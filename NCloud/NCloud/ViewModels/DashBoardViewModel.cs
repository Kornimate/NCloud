using Castle.Core;
using NCloud.ConstantData;
using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in DashBoard Index action method
    /// </summary>
    public class DashBoardViewModel
    {
        public List<string> SharedFolderUrls { get; set; }
        public List<string> SharedFileUrls { get; set; }
        public int UserPercent { get; set; }
        public int RemainingPercent { get; set; }
        public double UsedBytes { get; set; }
        public double RemainingBytes { get; set; }
        public Pair<string, string> WebControllerAndActionForDetails { get; set; }
        public Pair<string, string> WebControllerAndActionForDownload { get; set; }

        public DashBoardViewModel(List<string> sharedFoldersUrls, List<string> sharedFilesUrls, Pair<string,string> webControllerAndActionForDetails, Pair<string, string> webControllerAndActionForDownload, double userPercent, double usedBytes)
        {
            SharedFolderUrls = sharedFoldersUrls;
            SharedFileUrls = sharedFilesUrls;
            WebControllerAndActionForDetails = webControllerAndActionForDetails;
            WebControllerAndActionForDownload = webControllerAndActionForDownload;
            UserPercent = Convert.ToInt32(userPercent);
            RemainingPercent = (100 - UserPercent);
            UsedBytes = usedBytes;
            RemainingBytes = Constants.UserSpaceSize - usedBytes;
        }
    }
}
