namespace CCP.Shared.AuthContext
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        Guid OrganizationId { get; }
        bool IsServiceAccount { get; }

        string OrganizationName { get; }
        void SetCurrentUser(Guid userId);
        void SetIsServiceAccount(bool isServiceAccount);
        void SetOrganizationId(Guid organizationId);
        void SetOrganizationName(string organizationName);
    }
}
