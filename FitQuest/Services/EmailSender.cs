using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace FitQuest.Services
{
    public class EmailSettings
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }

    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new MailjetClient(_emailSettings.ApiKey, _emailSettings.ApiSecret);
            var request = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.FromEmail, _emailSettings.SenderEmail)
            .Property(Send.FromName, _emailSettings.SenderName)
            .Property(Send.Subject, subject)
            .Property(Send.HtmlPart, message)
            .Property(Send.Recipients, new JArray
            {
                new JObject
                {
                    { "Email", toEmail }
                }
            });

            await client.PostAsync(request);
        }
    }
}