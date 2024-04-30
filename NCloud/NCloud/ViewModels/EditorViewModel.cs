using Microsoft.AspNetCore.Mvc;

namespace NCloud.ViewModels
{
    public class EditorViewModel
    {
        public string FilePath { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty!;
        public string ExtensionData { get; set; } = String.Empty;
        public string Redirection { get; set; } = String.Empty;
    }
}
