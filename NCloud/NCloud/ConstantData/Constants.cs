using Castle.Core;

namespace NCloud.ConstantData
{
    public static class Constants
    {
        public static readonly List<string> SystemFolders = new List<string>()
        {
            "Music",
            "Documents",
            "Videos",
            "Pictures"
        };
        public static string AesKey { get => "bT7TAqTu32JxED0qTPac8w=="; }
        public static string IV { get => "IJ/UpiC2sYsmrq7fbFyemw=="; }
        public static string NoFileType { get => "notype"; }
        public static string ZipMimeType { get => "application/zip"; }
        public static string DefaultMimeType { get => "application/octet-stream"; }
        public static string DateTimeFormat { get => "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss"; }
        public static string UnkownFileType { get => "unknown"; }
        public static string FileTypePrefix { get => "filetype-"; }
        public static string IconsBasePath { get => Path.Combine("wwwroot", "utilities"); }
        public static string LogoPath { get => Path.Combine("wwwroot", "utilities", "cloud_logo.png"); }
        public static string TempFolderName { get => "temp"; }
        public static string TempFilePath { get => Path.Combine("wwwroot", TempFolderName); }
        public static string FolderIcon { get => "/utilities/folder.svg"; }
        public static string PrefixForIcons { get => "/utilities/filetype-"; }
        public static string SuffixForIcons { get => ".svg"; }
        public static string AdminUserName { get => "Admin"; }
        public static string AppName { get => "NCloudDrive"; }
        public static string PrivateRootName { get => "@CLOUDROOT"; }
        public static string PublicRootName { get => "@SHAREDROOT"; }
        public static string WebRootName { get => "@WEBROOT"; }
        public static string CompressedArchiveFileType { get => "zip"; }
        public static string NotSelectedResult { get => "false"; }
        public static char SelectedFileStarterSymbol { get => '_'; }
        public static char SelectedFolderStarterSymbol { get => '&'; }
        public static char FileNameDelimiter { get => '_'; }
        public static char PathSeparator { get => '/'; }
        public static int DistanceToRootFolder { get => 4; }
        public static int EmptyFolderAttributeNumberZip { get => 16; }
        public static int OwnerPlaceInPath { get => 1; }
        public static int GuidLength { get => 36; }
        public static TimeSpan TempFileDeleteTimeSpan { get => TimeSpan.FromMinutes(10); }

        public static Pair<string,string> GetWebControllerAndActionForDetails()
        {
            return new Pair<string, string>("Web","SharingPage");
        }

        public static Pair<string, string> GetWebControllerAndActionForDownload()
        {
            return new Pair<string, string>("Web", "DownloadPage");
        }

        public static string GetCloudRootPathInDatabase(Guid id)
        {
            return Path.Combine(PrivateRootName, id.ToString());
        }

        public static string GetSharingRootPathInDatabase(string userName)
        {
            return Path.Combine(PublicRootName, userName);
        }
    }
}
