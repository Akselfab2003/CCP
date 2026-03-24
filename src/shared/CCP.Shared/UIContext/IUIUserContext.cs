using CCP.Shared.ValueObjects;

namespace CCP.Shared.UIContext
{
    public interface IUIUserContext
    {
        Guid UserId { get; }
        string FirstName { get; }
        string LastName { get; }
        string FullName { get; }
        string Email { get; }
        string Initials { get; }
        Guid OrganizationId { get; }
        UserRole Role { get; }
        List<string> Roles { get; }
        bool IsInternalUser { get; }
        bool IsAuthenticated { get; }
    }
}
