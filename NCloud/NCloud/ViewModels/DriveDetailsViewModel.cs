using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DriveDetailsViewModel
    {
        public List<CloudRegistration?> items { get; set; }
        public int ParentId { get; set; }

        public DriveDetailsViewModel(List<CloudRegistration?> items,int parentId)
        {
            this.items = items;
            ParentId = parentId;
        }
    }
}
