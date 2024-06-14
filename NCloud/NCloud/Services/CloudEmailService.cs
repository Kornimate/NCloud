using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using NCloud.ConstantData;
using NCloud.Services.Exceptions;
using System.Net;
using System.Net.Mail;

namespace NCloud.Services
{
    public class CloudEmailService : IEmailSender
    {
        private readonly IConfiguration config;

        public CloudEmailService(IConfiguration config)
        {
            this.config = config;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                string emailAddress = config.GetSection("EmailCredentials:Email").Get<string>() ?? throw new CloudFunctionStopException("No email address provided to send data");
                string password = config.GetSection("EmailCredentials:Password").Get<string>() ?? throw new CloudFunctionStopException("No email password provided to send data");


                var msg = new MailMessage()
                {
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = htmlMessage,
                    To = { new MailAddress(email) },
                    From = new MailAddress(emailAddress),
                };

                var smtp = new SmtpClient(Constants.SmtpProvider)
                {
                    Port = 587,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(emailAddress, password)
                };

                return smtp.SendMailAsync(msg);
            }
            catch (Exception)
            {
                return new Task(() => { });
            }
        }
    }
}
