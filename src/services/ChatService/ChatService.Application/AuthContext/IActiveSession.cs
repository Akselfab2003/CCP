namespace ChatService.Application.AuthContext
{
    public interface IActiveSession
    {
        Guid SessionId { get; }
        Guid OrgId { get; }
        string Host { get; }

        void SetHost(string host);
        void SetOrgId(Guid orgId);
        void SetSessionId(Guid sessionId);
    }
}
