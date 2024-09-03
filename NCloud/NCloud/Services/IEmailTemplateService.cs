using Microsoft.AspNetCore.Identity.UI.Services;
using NCloud.Models;

namespace NCloud.Services
{
    public interface IEmailTemplateService : IEmailSender
    {
        Task SendEmailAsync(ICloudEmailTemplate emailTemplate);
        string GetSelfEmailAddress();
    }
}
