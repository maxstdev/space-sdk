using Maxst.Passport;

public abstract class PassportConfig
{
    public abstract ClientType clientType { get; }
    public abstract string Realm { get; }
    public abstract string ApplicationId { get; }
    public abstract string ApplicationKey { get; }
    public abstract string GrantType { get; }
}

