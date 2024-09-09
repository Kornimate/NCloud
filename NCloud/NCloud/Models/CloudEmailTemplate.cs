namespace NCloud.Models
{
    /// <summary>
    /// Template email interface for emails, uses the strategy design pattern
    /// </summary>
    public interface ICloudEmailTemplate
    {
        public abstract string GetTargetEmail();
        public abstract string GetSubject();
        public abstract string GetHtmlMessage();
    }

    /// <summary>
    /// Abstract class for template emails
    /// </summary>
    public abstract class CloudEmailTemplateAbstract : ICloudEmailTemplate
    {
        protected string? targetEmail;
        protected string? htmlMessage;

        public abstract string GetHtmlMessage();
        public abstract string GetSubject();
        public abstract string GetTargetEmail();
    }

    /// <summary>
    /// Registration template email content
    /// </summary>
    public class CloudUserRegistration : CloudEmailTemplateAbstract
    {
        public CloudUserRegistration(string targetEmail, string htmlMessage)
        {
            this.targetEmail = targetEmail;
            this.htmlMessage = htmlMessage;
        }

        public override string GetHtmlMessage()
        {
            return htmlMessage!;
        }

        public override string GetSubject()
        {
            return "#REGISTRATION";
        }

        public override string GetTargetEmail()
        {
            return targetEmail!;
        }
    }

    /// <summary>
    /// Account deletion template email content
    /// </summary>
    public class CloudUserAccountDeletion : CloudEmailTemplateAbstract
    {
        public CloudUserAccountDeletion(string targetEmail, string htmlMessage)
        {
            this.targetEmail = targetEmail;
            this.htmlMessage = htmlMessage;
        }

        public override string GetHtmlMessage()
        {
            return htmlMessage!;
        }

        public override string GetSubject()
        {
            return "#DELETE ACCOUNT";
        }

        public override string GetTargetEmail()
        {
            return targetEmail!;
        }
    }

    /// <summary>
    /// User locked out template email content
    /// </summary>
    public class CloudUserLockedOut : CloudEmailTemplateAbstract
    {
        public CloudUserLockedOut(string targetEmail, string htmlMessage)
        {
            this.targetEmail = targetEmail;
            this.htmlMessage = htmlMessage;
        }

        public override string GetHtmlMessage()
        {
            return htmlMessage!;
        }

        public override string GetSubject()
        {
            return "#ACCOUNT LOCKED OUT";
        }

        public override string GetTargetEmail()
        {
            return targetEmail!;
        }
    }

    /// <summary>
    /// Space request submitted template email content
    /// </summary>
    public class CloudUserSpaceRequest : CloudEmailTemplateAbstract
    {
        public CloudUserSpaceRequest(string targetEmail, string htmlMessage)
        {
            this.targetEmail = targetEmail;
            this.htmlMessage = htmlMessage;
        }

        public override string GetHtmlMessage()
        {
            return htmlMessage!;
        }

        public override string GetSubject()
        {
            return "#NEW SPACE REQUEST";
        }

        public override string GetTargetEmail()
        {
            return targetEmail!;
        }
    }
}
