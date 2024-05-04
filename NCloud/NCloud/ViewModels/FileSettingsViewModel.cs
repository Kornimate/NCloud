using NCloud.ConstantData;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class FileSettingsViewModel
    {
        [Required]
        public string? OldName { get; set; }

        [Required(ErrorMessage = "File Name is compulsory")]
        [Display(Name = "Name")]
        public string? NewName { get; set; }

        [Required]
        public string? Extension { get; set; }

        [Required]
        public string? Path { get; set; }

        [Required]
        [Display(Name = $"Shared in App")]
        public bool ConnectedToApp { get; set; } = false;

        [Required]
        [Display(Name = $"Shared on Web")]

        public bool ConnectedToWeb { get; set; } = false;

        public FileInfo? Info { get; set; }

        public FileSettingsViewModel(string oldName, string newName, string extension, string path, bool connectedToApp, bool connectedToWeb, FileInfo info)
        {
            OldName = oldName;
            NewName = newName;
            Extension = extension;
            Path = path;
            ConnectedToApp = connectedToApp;
            ConnectedToWeb = connectedToWeb;
            Info = info;
        }

        public FileSettingsViewModel() { }
    }
}
