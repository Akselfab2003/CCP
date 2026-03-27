using MailKit;
using MailKit.Net.Imap;

namespace EmailService.Worker.Host;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    private CancellationTokenSource _cancelToken = new CancellationTokenSource();
    private CancellationTokenSource _doneToken = new CancellationTokenSource();
    private ImapClient _idleClient = new ImapClient();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancelToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _idleClient = new ImapClient();


        await _idleClient.ConnectAsync("localhost", 143, false);
        await _idleClient.AuthenticateAsync(configuration.GetValue<string>("emailWorkerServiceUsername"), configuration.GetValue<string>("emailWorkerServicePassword"));
        await _idleClient.Inbox.OpenAsync(FolderAccess.ReadOnly, CancellationToken.None);

        _idleClient.Inbox.CountChanged += OnCountChanged;
        await RunIdleLoopAsync(_cancelToken.Token);
        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    imapMailReciver.ConnectAsync().GetAwaiter().GetResult();
        //    //imapMailReciver.ListenerAsync().GetAwaiter().GetResult();
        //    if (logger.IsEnabled(LogLevel.Information))
        //    {
        //        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //    }
        //    await Task.Delay(1000, stoppingToken);
        //}
    }
    private async Task RunIdleLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (_doneToken = new CancellationTokenSource())
            {
                // IDLE times out after ~29 min — restart it to keep the connection alive
                var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(29));

                // Exit IDLE when timeout hits or a new message arrives
                timeout.Token.Register(() => _doneToken.Cancel());

                try
                {
                    // _idleClient.Capabilities.HasFlag(ImapCapabilities.Idle);
                    //if (_idleClient.Capabilities.HasFlag(ImapCapabilities.Idle))
                    //{
                    await _idleClient.IdleAsync(_doneToken.Token, cancellationToken);
                    //}
                    //else
                    //{
                    //    // Fallback: poll every 30 seconds if IDLE not supported
                    //    await Task.Delay(TimeSpan.FromSeconds(30), _doneToken.Token);
                    //    await _idleClient.NoOpAsync(cancellationToken);
                    //}
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
    private async void OnCountChanged(object? sender, EventArgs e)
    {

        using (var client = new ImapClient())
        {
            client.Connect("localhost", 143, false);
            client.Authenticate(configuration.GetValue<string>("emailWorkerServiceUsername"), configuration.GetValue<string>("emailWorkerServicePassword"));


            var FolderWithChange = sender as IMailFolder;

            if (FolderWithChange == null)
            {
                return;
            }

            var folders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);

            var folder = folders.FirstOrDefault(f => f.FullName == FolderWithChange.FullName);

            if (folder == null)
            {
                return;
            }
            await folder.OpenAsync(FolderAccess.ReadOnly);
            // Fetch only the new messages (from the last known count onward)
            var arrived = folder.FetchAsync(
                0, -1,
                MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope,
                _cancelToken.Token
            ).GetAwaiter().GetResult();

            foreach (var summary in arrived)
            {
                // Fetch the full message
                var message = folder.GetMessageAsync(summary.UniqueId, _cancelToken.Token).GetAwaiter().GetResult();
                if (message != null)
                {
                    logger.LogInformation("New mail received at: {time}", DateTimeOffset.Now);
                    logger.LogInformation("Mail subject: {subject}", message.Subject);
                    logger.LogInformation("Mail date: {date}", message.Date);
                    logger.LogInformation("Mail body: {body}", message.TextBody);
                    logger.LogInformation("Normalized subject: {normalizedSubject}", summary.NormalizedSubject);
                }
            }
        }

        // Exit IDLE so the loop restarts with fresh state
        _doneToken?.Cancel();
    }

    private void OnMessageExpunged(object sender, MessageEventArgs e)
    {
        Console.WriteLine($"Message at index {e.Index} was deleted.");
        _doneToken?.Cancel();
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

