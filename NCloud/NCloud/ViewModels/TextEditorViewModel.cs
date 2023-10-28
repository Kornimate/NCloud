using System.Web.Mvc;

namespace NCloud.ViewModels
{
    public class TextEditorViewModel
    {
        [AllowHtml]
        public string? Text { get; set; } = String.Empty;
    }
}
