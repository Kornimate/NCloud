using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in DashBoard Index action method
    /// </summary>
    public class DashBoardViewModel
    {
        public List<SharedFolder> WebSharedFolderData { get; set; }
        public List<SharedFile> WebSharedFileData { get; set; }
        public int UserPercent { get; set; }
        public int RemainingPercent { get; set; }
        public double UsedBytes { get; set; }
        public double RemainingBytes { get; set; }
        public Pair<string, string> WebControllerAndActionForDetails { get; set; }
        public Pair<string, string> WebControllerAndActionForDownload { get; set; }

        public DashBoardViewModel(List<SharedFolder> sharedFoldersData, List<SharedFile> sharedFilesData, Pair<string, string> webControllerAndActionForDetails, Pair<string, string> webControllerAndActionForDownload, double userPercent, double usedBytes, double maxBytes)
        {
            WebSharedFolderData = sharedFoldersData;
            WebSharedFileData = sharedFilesData;
            WebControllerAndActionForDetails = webControllerAndActionForDetails;
            WebControllerAndActionForDownload = webControllerAndActionForDownload;
            UserPercent = Convert.ToInt32(userPercent);
            RemainingPercent = (100 - UserPercent);
            UsedBytes = usedBytes;
            RemainingBytes = maxBytes - usedBytes;
        }
    }
}
