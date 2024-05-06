using NCloud.ConstantData;
using NCloud.Security;

namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public string? SharedName { get; set; }
        public string? HashedPath { get; set; }
        public string? ItemPath { get; set; }
        public bool IsConnectedToApp { get; set; }
        public bool IsConnectedToWeb { get; set; }
        
        public CloudRegistration(bool isSharedInApp, bool isPublic, string? currentPath = null)
        {
            IconPath = null!;
            IsConnectedToApp = isSharedInApp;
            IsConnectedToWeb = isPublic;
            HashedPath = HashManager.EncryptString(currentPath);
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
        public abstract string ReturnName();
        
        public static CloudRegistration? RegistrationPathFactory(string clipBoardData)
        {
            if (clipBoardData.Length < 2)
                return null;

            if (clipBoardData.StartsWith(Constants.SelectedFileStarterSymbol))
            {
                return new CloudFile(clipBoardData[2..]);
            }
            else if (clipBoardData.StartsWith(Constants.SelectedFolderStarterSymbol))
            {
                return new CloudFolder(clipBoardData[2..]);
            }

            return null;
        }
    }
}
