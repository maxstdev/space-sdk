using Maxst.Passport;

namespace MaxstXR.Place
{

    public class SpaceSDKSampleAuthConfig : PassportConfig
    {
        public override ClientType clientType => ClientType.Public;

        public override string Realm => "maxst";

        public override string ApplicationId => "fcb71831-975e-4705-836f-b55f90f87515";

        public override string ApplicationKey => "7sjVDHJY9X4kj9nsa5XjxOrzvaYn0H1q";

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
