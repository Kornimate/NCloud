using NCloud.ConstantData;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class FolderSettingsViewModel
    {
        [Required(ErrorMessage = "Folder Name is compulsory")]
        public string Name { get; set; }
        public string Path { get; set; }

        [Required]
        [Display(Name = $"Shared in App")]
        public bool ConnectedToApp { get; set; }

        [Required]
        [Display(Name = $"Shared on Web")]

        public bool ConnectedToWeb { get; set; }

        public FolderSettingsViewModel(string name, string path, bool connectedToApp, bool connectedToWeb)
        {
            Name = name;
            Path = path;
            ConnectedToApp = connectedToApp;
            ConnectedToWeb = connectedToWeb;
        }
    }
}
