using Cysharp.Threading.Tasks;
using Maxst.Passport;
using MaxstUtils;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class XRTokenManager
{
    private const string TOKEN_TYPE = "Bearer ";
    private static volatile XRTokenManager instance;
    public string authorization;
    private static readonly object lockObj = new Object();
    public string contentType => "application/json";
    private XRTokenManager() { }
    public static XRTokenManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new XRTokenManager();
                    }
                }
            }
            return instance;
        }
    }

    public async UniTask<string> GetActiveToken(PassportConfig config = null, bool attachTokenType = true)//If there is no passport config, it means only usertoken is used.
    {
        var tokenRepo = TokenRepo.Instance;
        var userToken = tokenRepo.GetToken();
        if (IsUserTokenValid(userToken))
        {
            Debug.Log($"XRTokenManager: userToken is Valid : {userToken.accessToken}");
            authorization = FormatToken(userToken.accessToken);
            return authorization;
        }

        if (config == null)
        {
            //TODO Get UserToken logic
            Debug.Log($"XRTokenManager: userToken Empty");
            return null;
        }
        
        var clientToken = await GetOrCreateClientToken(tokenRepo, config);
        if (clientToken != null)
        {
            if(attachTokenType)
            {
                authorization = FormatToken(clientToken.access_token);
                return authorization;
            }
            return clientToken.access_token;
        }
        Debug.Log($"XRTokenManager: client token is Empty");
        return null;
    }
    private string FormatToken(string accessToken)
    {
        return TOKEN_TYPE + accessToken;
    }

    private bool IsUserTokenValid(Token userToken)
    {
        return userToken != null && !TokenRepo.Instance.IsTokenExpired();
    }

    private async UniTask<ClientToken> GetOrCreateClientToken(TokenRepo tokenRepo, PassportConfig config)
    {
        var clientToken = tokenRepo.GetClientToken();
        if (clientToken == null || tokenRepo.ClientIsTokenExpired())
        {
            Debug.Log($"XRTokenManager: client token is not valid and Fetch Token");
            await tokenRepo.GetPassportClientToken(
                config.ApplicationId,
                config.ApplicationKey,
                config.GrantType,
                (_, token) => clientToken = token,
                null,
                false);
        }
        //Debug.Log($"XRTokenManager: get client token : {clientToken.access_token}");
        return clientToken;
    }

    public Dictionary<string, string> GetHeaders()
    {
        return new Dictionary<string, string>()
        {
            { "Authorization",  authorization },
            { "Content-Type", contentType },
        };
    }
}
