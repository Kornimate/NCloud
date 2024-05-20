namespace NCloud.Models
{
    /// <summary>
    /// Class to handle url generation for client side commands
    /// </summary>
    public class UrlGenerationResult
    {
        public string Controller {  get; set; }
        public string Action { get; set; }
        public object Parameters { get; set; }
        public bool Downloadable { get; set; }

        public UrlGenerationResult(string controller, string action, object parameters, bool downloadable)
        {
            Controller = controller;
            Action = action;
            Parameters = parameters;
            Downloadable = downloadable;
        }
    }
}
