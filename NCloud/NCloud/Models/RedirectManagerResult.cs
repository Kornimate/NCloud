namespace NCloud.Models
{
    /// <summary>
    /// Class to wrap result of RedirectionManager CreatedRedirectionAction method
    /// </summary>
    public class RedirectManagerResult
    {
        public string Controller { get; set; }
        public string Action { get; set; }

        public RedirectManagerResult(string controller, string action)
        {
            Controller = controller;
            Action = action;
        }
    }
}
