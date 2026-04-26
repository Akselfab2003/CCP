namespace MailCow.Sdk.services.MailBox
{
    public interface IMailBoxManagementService
    {
        Task<Result> AddMailBox(string mailBoxName, string Domain, string Password, CancellationToken ct = default);
    }
}
