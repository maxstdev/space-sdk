using Maxst.Passport;

namespace MaxstXR.Place
{

    public class SpaceSDKSampleAuthConfigAlpha : PassportConfig
    {
        public override ClientType clientType => ClientType.Public;

        public override string Realm => "maxst";
        public override string ApplicationId => "";

        public override string ApplicationKey => "";

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
