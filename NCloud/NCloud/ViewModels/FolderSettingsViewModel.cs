using NCloud.ConstantData;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class FolderSettingsViewModel
    {
        [Required]
        public string OldName { get; set; }

        [Required(ErrorMessage = "Folder Name is compulsory")]
        [Display(Name = "Name")]
        public string NewName { get; set; }
        public string Path { get; set; }

        [Required]
        [Display(Name = $"Shared in App")]
        public bool ConnectedToApp { get; set; }

        [Required]
        [Display(Name = $"Shared on Web")]

        public bool ConnectedToWeb { get; set; }

        public FolderSettingsViewModel(string oldName, string newName, string path, bool connectedToApp, bool connectedToWeb)
        {
            OldName = oldName;
            NewName = newName;
            Path = path;
            ConnectedToApp = connectedToApp;
            ConnectedToWeb = connectedToWeb;
        }
    }
}
