using Maxst.Passport;

namespace MaxstXR.Place
{

    public class SpaceSDKSampleAuthConfig : PassportConfig
    {
        public override ClientType clientType => ClientType.Public;

        public override string Realm => "maxst";

        public override string ApplicationId => "";

        public override string ApplicationKey => "";

        public override string GrantType => "client_credentials";

        private static SpaceSDKSampleAuthConfig instance;
        public static SpaceSDKSampleAuthConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpaceSDKSampleAuthConfig();

                }
                return instance;
            }
        }
    }
}
