using Castle.Core;
using NCloud.Models;

namespace NCloud.ViewModels
{
    public class DashBoardViewModel
    {
        public List<string> SharedFolderUrls { get; set; }
        public List<string> SharedFileUrls { get; set; }
        public Pair<string, string> WebControllerAndActionForDetails { get; set; }
        public Pair<string, string> WebControllerAndActionForDownload { get; set; }

        public DashBoardViewModel(List<string> sharedFoldersUrls, List<string> sharedFilesUrls, Pair<string,string> webControllerAndActionForDetails, Pair<string, string> webControllerAndActionForDownload)
        {
            SharedFolderUrls = sharedFoldersUrls;
            SharedFileUrls = sharedFilesUrls;
            WebControllerAndActionForDetails = webControllerAndActionForDetails;
            WebControllerAndActionForDownload = webControllerAndActionForDownload;
        }
    }
}
