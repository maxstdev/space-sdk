using Maxst.Passport;

public class SampleConfig : PassportConfig
{
    public override ClientType clientType => ClientType.Public;

    public override string Realm => "maxst";

    public override string ApplicationId => "";

    public override string ApplicationKey => "";

    public override string GrantType => "client_credentials";

    private static SampleConfig instance;
    public static SampleConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SampleConfig();

            }
            return instance;
        }
    }
}
