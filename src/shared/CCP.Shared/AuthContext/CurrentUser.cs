namespace CCP.Shared.AuthContext
{
    public class CurrentUser : ICurrentUser
    {
        public Guid UserId { get; private set; }
        public Guid OrganizationId { get; private set; }
        public bool IsServiceAccount { get; private set; }

        public string OrganizationName { get; private set; } = string.Empty;

        public void SetCurrentUser(Guid userId)
        {
            UserId = userId;
        }

        public void SetIsServiceAccount(bool isServiceAccount)
        {
            IsServiceAccount = isServiceAccount;
        }

        public void SetOrganizationId(Guid organizationId)
        {
            OrganizationId = organizationId;
        }

        public void SetOrganizationName(string organizationName)
        {
            OrganizationName = organizationName;
        }
    }
}
