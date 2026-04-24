namespace CCP.Shared.AuthContext
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        Guid OrganizationId { get; }
        bool IsServiceAccount { get; }

        void SetCurrentUser(Guid userId);
        void SetOrganizationId(Guid organizationId);
        void SetIsServiceAccount(bool isServiceAccount);
    }
}
