using System.Globalization;
using System.Text;
using System.Threading.Channels;
using MailKit.Security;
using Microsoft.Extensions.Localization;
using MimeKit;
namespace Caffeine.Services;

public sealed record ResetMail(string Recipient, string Url, string Culture, DateTimeOffset? ExpiresAt = null);
public interface IEmailSender
{
    bool IsConfigured { get; }
    Task SendResetAsync(ResetMail mail, CancellationToken cancellationToken);
}
public sealed class SmtpEmailSender(IConfiguration config, IStringLocalizer<SharedResource> text) : IEmailSender
{
    public static SecureSocketOptions SecurityMode(string? mode) => mode switch {
        null or "StartTls" => SecureSocketOptions.StartTls,
        "SslOnConnect" => SecureSocketOptions.SslOnConnect,
        _ => throw new ArgumentException("Only mandatory TLS modes are supported.")
    };
    public bool IsConfigured => config.GetValue<bool>("Email:Enabled") &&
        !string.IsNullOrWhiteSpace(config["Email:Host"]) &&
        !(config["Email:Host"]?.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)) ?? true) &&
        MailboxAddress.TryParse(config["Email:From"], out _) &&
        config.GetValue("Email:Port", 587) is >= 1 and <= 65535 &&
        (config["Email:Security"] is null or "StartTls" or "SslOnConnect") &&
        (config.GetValue<bool>("Email:AllowAnonymous") || (!string.IsNullOrWhiteSpace(config["Email:Username"]) && !string.IsNullOrWhiteSpace(config["Email:Password"])));

    public MimeMessage CreateMessage(ResetMail mail)
    {
        if (!Uri.TryCreate(mail.Url, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ArgumentException("Invalid reset URL.");
        var oldCulture = CultureInfo.CurrentUICulture;
        try {
            CultureInfo.CurrentUICulture = new CultureInfo(mail.Culture is "hu" or "de" ? mail.Culture : "en");
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(config["Email:From"]!));
            message.To.Add(MailboxAddress.Parse(mail.Recipient));
            message.Subject = text["ResetEmailSubject"];
            message.Body = new TextPart("plain") { Text = text["ResetEmailIntro"] + "\n\n" + mail.Url + "\n\n" + text["ResetEmailIgnore"] };
            return message;
        } finally { CultureInfo.CurrentUICulture = oldCulture; }
    }
    public async Task SendResetAsync(ResetMail mail, CancellationToken cancellationToken)
    {
        if (!IsConfigured) throw new InvalidOperationException("SMTP is not configured.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        using var client = new MailKit.Net.Smtp.SmtpClient { Timeout = 15000 };
        // Default certificate verification stays enabled. No plaintext or opportunistic TLS.
        await client.ConnectAsync(config["Email:Host"]!, config.GetValue("Email:Port", 587), SecurityMode(config["Email:Security"]), timeout.Token);
        if (!config.GetValue<bool>("Email:AllowAnonymous"))
            await client.AuthenticateAsync(config["Email:Username"]!, config["Email:Password"]!, timeout.Token);
        await client.SendAsync(CreateMessage(mail), timeout.Token);
        await client.DisconnectAsync(true, timeout.Token);
    }
}
public sealed class ResetMailQueue
{
    private readonly Channel<ResetMail> channel = Channel.CreateBounded<ResetMail>(new BoundedChannelOptions(100) {
        FullMode = BoundedChannelFullMode.Wait, SingleReader = true });
    public bool TryQueue(ResetMail mail) => channel.Writer.TryWrite(mail);
    public IAsyncEnumerable<ResetMail> ReadAll(CancellationToken token) => channel.Reader.ReadAllAsync(token);
}
public sealed class ResetEmailWorker(ResetMailQueue queue, IEmailSender sender, TimeProvider time,
    ILogger<ResetEmailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var mail in queue.ReadAll(stoppingToken)) {
            for (var attempt = 0; attempt < 3; attempt++) {
                if (mail.ExpiresAt.HasValue && mail.ExpiresAt.Value <= time.GetUtcNow()) break;
                try { await sender.SendResetAsync(mail, stoppingToken); break; }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception) {
                    // Never log the exception: transport messages may include recipients or credentials.
                    if (attempt == 2) logger.LogWarning("Password-reset delivery failed after three attempts. Check SMTP configuration.");
                    else await Task.Delay(TimeSpan.FromSeconds(2 * (attempt + 1)), time, stoppingToken);
                }
            }
        }
    }
}
