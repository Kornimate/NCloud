using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class EditorIndexViewModel
    {
        public SelectList? CodingExtensions { get; set; }
        public SelectList? TextDocumentExtensions { get; set; }

        [Required(ErrorMessage = "File name is compulsory")]
        public string FileName { get; set; } = String.Empty;

        [Required(ErrorMessage = "Extension is compulsory")]
        public string Extension { get; set; } = String.Empty;
    }
}
