using Maxst.Passport;



public class XRSDKAuthConfig : PassportConfig
{
    public override ClientType clientType => ClientType.Public;

    public override string Realm => "maxst";

    public override string ApplicationId => "";

    public override string ApplicationKey => "";

    public override string GrantType => "client_credentials";

    private static XRSDKAuthConfig instance;
    public static XRSDKAuthConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new XRSDKAuthConfig();

            }
            return instance;
        }
    }
}

