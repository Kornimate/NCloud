namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in to present error
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}