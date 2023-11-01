using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace Maxst.Passport
{
    public interface IAuthApi
    {
        [Obsolete]
        [Post("/passport/token")]
        IObservable<ClientToken> PassportClientToken(
        [Field("client_id")] string applicationId,
        [Field("client_secret")] string applicationKey,
        [Field("grant_type")] string grantType);

        [Post("/passport/token")]
        IObservable<CredentialsToken> ConfidentialPassportToken([Field("client_id")] string client_id,
        [Field("client_secret")] string client_secret,
        [Field("grant_type")] string grant_type,
        [Field("redirect_uri")] string redirect_uri,
        [Field("code")] string code);

        [Post("/passport/token")]
        IObservable<CredentialsToken> PassportToken([Field("client_id")] string client_id,
        [Field("code_verifier")] string code_verifier,
        [Field("grant_type")] string grant_type,
        [Field("redirect_uri")] string redirect_uri,
        [Field("code")] string code);

        [Post("/passport/token")]
        IObservable<CredentialsToken> PassportRefreshToken(
        [Field("client_id")] string client_id,
        [Field("grant_type")] string grant_type,
        [Field("refresh_token")] string refresh_token
        );
        
        [Post("/passport/token")]
        IObservable<CredentialsToken> PassportConfidentialRefreshToken(
        [Field("client_id")] string client_id,
        [Field("grant_type")] string grant_type,
        [Field("refresh_token")] string refresh_token,
        [Field("client_secret")] string client_secret
        );

        [Post("/passport/connect/logout")]
        IObservable<string> PassportLogout(
        [Header("Authorization")] string accessToken,
        [Field("client_id")] string client_id,
        [Field("id_token_hint")] string id_token);
    }
}
