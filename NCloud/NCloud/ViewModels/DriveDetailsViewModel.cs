using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveDetailsViewModel
    {
        public List<CloudRegistration?> items { get; set; }

        public DriveDetailsViewModel(List<CloudRegistration?> items)
        {
            this.items = items;
        }
    }
}
