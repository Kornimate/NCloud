namespace NCloud.Models
{
    public interface ICloudEmailTemplate
    {
        public abstract string GetTargetEmail();
        public abstract string GetSubject();
        public abstract string GetHtmlMessage();
    }

    public abstract class CloudEmailTemplateAbstract : ICloudEmailTemplate
    {
        protected string? targetEmail;
        protected string? htmlMessage;

        public abstract string GetHtmlMessage();
        public abstract string GetSubject();
        public abstract string GetTargetEmail();
    }

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
}
