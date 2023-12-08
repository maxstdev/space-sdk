using Maxst.Passport;

namespace MaxstXR.Place
{

    public class SpaceSDKSampleAuthConfigAlpha : PassportConfig
    {
        public override ClientType clientType => ClientType.Public;

        public override string Realm => "maxst";
        public override string ApplicationId => "463f5f30-6226-4242-aa94-e73e1cad8999";

        public override string ApplicationKey => "Fqq8rGEm2OHK0gWhQeWFD7RZq2aqzIH5";

        public override string GrantType => "client_credentials";

        private static SpaceSDKSampleAuthConfigAlpha instance;
        public static SpaceSDKSampleAuthConfigAlpha Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpaceSDKSampleAuthConfigAlpha();

                }
                return instance;
            }
        }
    }
}
