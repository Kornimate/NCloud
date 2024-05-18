namespace NCloud.Models
{
    /// <summary>
    /// Class to show error in UI
    /// </summary>
    public class Error : CloudNotificationAbstarct
    {
        public Error(string? text) : base("bg-danger", @"/utilities/error.svg", "Error", 1, text) { }
    }

    /// <summary>
    /// Class to show warning in UI
    /// </summary>
    public class Warning : CloudNotificationAbstarct
    {
        public Warning(string? text) : base("bg-warning", @"/utilities/warning.svg", "Warning", 2, text) { }
    }

    /// <summary>
    /// Class to show success in UI
    /// </summary>
    public class Success : CloudNotificationAbstarct
    {
        public Success(string? text) : base("bg-success", @"/utilities/success.svg", "Success", 3, text) { }
    }

    /// <summary>
    /// Class to show information in UI
    /// </summary>
    public class Information : CloudNotificationAbstarct
    {
        public Information(string? text) : base("bg-info", @"/utilities/info.svg", "Information", 4, text) { }
    }
}
