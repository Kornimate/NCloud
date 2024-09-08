using Microsoft.AspNetCore.Identity.UI.Services;
using NCloud.Models;

namespace NCloud.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private IEmailSender emailSender;
        private IConfiguration config;

        public EmailTemplateService(IEmailSender emailSender, IConfiguration config)
        {
            this.emailSender = emailSender;
            this.config = config;
        }

        public string GetSelfEmailAddress()
        {
            return config.GetSection("EmailCredentials:Email").Get<string>() ?? String.Empty;
        }

        public Task SendEmailAsync(ICloudEmailTemplate emailTemplate)
        {
            return emailSender.SendEmailAsync(emailTemplate.GetTargetEmail(), emailTemplate.GetSubject(), emailTemplate.GetHtmlMessage());
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
