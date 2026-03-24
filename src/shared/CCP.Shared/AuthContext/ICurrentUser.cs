namespace CCP.Shared.AuthContext
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        Guid OrganizationId { get; }

        void SetCurrentUser(Guid userId);
        void SetOrganizationId(Guid organizationId);
    }
}
