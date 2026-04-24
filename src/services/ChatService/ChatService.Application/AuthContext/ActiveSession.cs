namespace ChatService.Application.AuthContext
{
    public class ActiveSession : IActiveSession
    {
        public Guid SessionId { get; private set; }
        public Guid OrgId { get; private set; }
        public string Host { get; private set; } = string.Empty;
        public void SetSessionId(Guid sessionId) => SessionId = sessionId;
        public void SetOrgId(Guid orgId) => OrgId = orgId;
        public void SetHost(string host) => Host = host;
    }
}
