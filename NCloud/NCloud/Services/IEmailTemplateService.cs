using Microsoft.AspNetCore.Identity.UI.Services;
using NCloud.Models;

namespace NCloud.Services
{
    /// <summary>
    /// Interface to handle cloud email sending service
    /// </summary>
    public interface IEmailTemplateService : IEmailSender
    {
        /// <summary>
        /// Method to send email using a template
        /// </summary>
        /// <param name="emailTemplate">Template of the email</param>
        /// <returns>Task to be awaited</returns>
        Task SendEmailAsync(ICloudEmailTemplate emailTemplate);

        /// <summary>
        /// Method to get google smtp email address for authorization
        /// </summary>
        /// <returns></returns>
        string GetSelfEmailAddress();
    }
}
