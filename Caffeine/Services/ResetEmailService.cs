using System.Net;
using System.Net.Mail;
using System.Threading.Channels;

namespace Caffeine.Services;

public sealed record ResetMail(
    string Recipient,
    string Url,
    string Culture);

public interface IEmailSender
{
    bool IsConfigured { get; }

    Task SendResetAsync(
        ResetMail mail,
        CancellationToken cancellationToken);
}

public sealed class SmtpEmailSender(
    IConfiguration config)
    : IEmailSender
{
    public bool IsConfigured =>
        config.GetValue<bool>("Email:Enabled") &&
        !string.IsNullOrWhiteSpace(
            config["Email:Host"]) &&
        !string.IsNullOrWhiteSpace(
            config["Email:From"]);

    public async Task SendResetAsync(
        ResetMail mail,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            throw new InvalidOperationException(
                "Email is not configured.");

        using var client =
            new SmtpClient(
                config["Email:Host"]!,
                config.GetValue(
                    "Email:Port",
                    587))
            {
                EnableSsl = true,

                UseDefaultCredentials = false,

                Credentials =
                    new NetworkCredential(
                        config["Email:Username"],
                        config["Email:Password"]),

                Timeout = 15000
            };

        var hu =
            mail.Culture == "hu";

        using var message =
            new MailMessage(
                config["Email:From"]!,
                mail.Recipient)
            {
                Subject =
                    hu
                        ? "Koffi – jelszó-visszaállítás"
                        : "Koffi – password reset",

                Body =
                    (hu
                        ? "A jelszó-visszaállító linked 30 percig érvényes, és egyszer használható:\n\n"
                        : "Your password reset link is valid for 30 minutes and can be used once:\n\n")
                    +
                    mail.Url
                    +
                    (hu
                        ? "\n\nHa nem te kérted, hagyd figyelmen kívül ezt az emailt."
                        : "\n\nIf you did not request this, ignore this email."),

                IsBodyHtml = false
            };

        await client.SendMailAsync(
            message,
            cancellationToken);
    }
}

public sealed class ResetMailQueue
{
    private readonly Channel<ResetMail> channel =
        Channel.CreateBounded<ResetMail>(
            new BoundedChannelOptions(100)
            {
                FullMode =
                    BoundedChannelFullMode.Wait,

                SingleReader = true
            });

    public bool TryQueue(
        ResetMail mail) =>
        channel.Writer.TryWrite(mail);

    public IAsyncEnumerable<ResetMail> ReadAll(
        CancellationToken token) =>
        channel.Reader.ReadAllAsync(token);
}

public sealed class ResetEmailWorker(
    ResetMailQueue queue,
    IEmailSender sender,
    ILogger<ResetEmailWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await foreach (
            var mail in queue.ReadAll(stoppingToken))
        {
            try
            {
                await sender.SendResetAsync(
                    mail,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Password-reset email delivery failed. " +
                    "Check SMTP configuration. " +
                    "No token or recipient was logged.");
            }
        }
    }
}