using System;

namespace Maxst.Passport
{
    public interface IOpenIDConnectListener
    {
        void OnSuccess(Token Token, RequestType type);
        void OnFail(ErrorCode ErrorCode, Exception e);

        void OnLogout();
    }
}
