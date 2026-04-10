using MailKit;
using MailKit.Net.Imap;

namespace EmailService.Worker.Host;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private CancellationTokenSource _cancelToken = new CancellationTokenSource();
    private CancellationTokenSource _doneToken = new CancellationTokenSource();
    private ImapClient _idleClient = new ImapClient();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }

    }


    public async void MailCountChange(object? sender, EventArgs e)
    {
        var inbox = sender as IMailFolder;
        var mails = await inbox.FetchAsync(0, -1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId);
        foreach (var item in mails)
        {
            logger.LogInformation("New mail received at: {time}", DateTimeOffset.Now);
            logger.LogInformation("Mail subject: {subject}", item.NormalizedSubject);
            logger.LogInformation("Mail date: {date}", item.Date);
            logger.LogInformation("Mail body: {body}", item.Body);
            logger.LogInformation("Normalized subject: {normalizedSubject}", item.NormalizedSubject);
        }
    }
}

