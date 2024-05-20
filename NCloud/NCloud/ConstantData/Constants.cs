using Castle.Core;
using NCloud.Users;
using System.Security.Claims;

namespace NCloud.ConstantData
{
    /// <summary>
    /// Class to store constants used application wide
    /// </summary>
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
        public static string FolderAndFileRegex { get => @"^[0-9a-zA-Z_!%+=.()$\s]+$"; }
        public static string CommandRegex { get => @"^[0-9a-zA-Z_!%+=.()-/$\s""@]+$"; }
        public static string UserRegex { get => @"^[0-9a-zA-Z_!%+=.()/$\s""]+$"; }
        public static string ZipMimeType { get => "application/zip"; }
        public static string DefaultMimeType { get => "application/octet-stream"; }
        public static string DefaultFileName { get => "<invalid-name>"; }
        public static string DateTimeFormat { get => "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss"; }
        public static string TerminalDateTimeFormat { get => "yyyy'-'MM'-'dd' 'HH':'mm"; }
        public static string UnkownFileType { get => "unknown"; }
        public static string FileTypePrefix { get => "filetype-"; }
        public static string WebRootFolderName { get => ".__CloudData__"; }
        public static string IconsBasePath { get => Path.Combine("wwwroot", "utilities"); }
        public static string LogoPath { get => Path.Combine("wwwroot", "utilities", "cloud_logo.png"); }
        public static string TempFolderName { get => "temp"; }
        public static string TempFilePath { get => Path.Combine(".__TempFiles__", TempFolderName); }
        public static string FolderIcon { get => "/utilities/folder.svg"; }
        public static string PrefixForIcons { get => "/utilities/filetype-"; }
        public static string SuffixForIcons { get => ".svg"; }
        public static string AdminUserName { get => "Admin"; }
        public static string AppName { get => "NCloudDrive"; }
        public static string PrivateRootName { get => "@CLOUDROOT"; }
        public static string PublicRootName { get => "@SHAREDROOT"; }
        public static string CompressedArchiveFileType { get => "zip"; }
        public static string NotSelectedResult { get => "false"; }
        public static string CodingExtensionsFilePath { get => "./Services/Resources/coding-extensions.json"; }
        public static string TextDocumentExtensionsFilePath { get => "./Services/Resources/text-document-extensions.json"; }
        public static string TerminalCommandsDataFilePath { get => "./Services/Resources/terminal-commands.json"; }
        public static string ControllerDataSeparator { get => "||"; }
        public static string NotificationCookieKey { get => "Notification"; }
        public static string DownloadHTMLElementId { get => "actionTrigger"; }
        public static string CloudCookieKey { get => "pathDataCloud"; }
        public static string SharedCookieKey { get => "pathDataShared"; }
        public static string TerminalHelpName { get => "name"; }
        public static string TerminalHelpDescription { get => "description"; }
        public static string DirectoryBack { get => ".."; }
        public static string UserDataSeparator { get => "\\"; }
        public static char SelectedFileStarterSymbol { get => '@'; }
        public static char SelectedFolderStarterSymbol { get => '&'; }
        public static char SingleLineCommandMarker { get => '@'; }
        public static char AbsolutePathMarker { get => '@'; }
        public static char FileNameDelimiter { get => '_'; }
        public static char FileExtensionDelimiter { get => '.'; }
        public static char CommandCurrentPathMarker { get => '.'; }
        public static char PathSeparator { get => '/'; }
        public static char TerminalStringMarker { get => '\"'; }
        public static char TerminalWhiteSpace { get => ' '; }
        public static int DistanceToRootFolder { get => 3; }
        public static int EmptyFolderAttributeNumberZip { get => 16; }
        public static int OwnerPlaceInPath { get => 1; }
        public static int RootProviderPlaceinPath { get => 0; }
        public static int GuidLength { get => 36; }
        public static double UserSpaceSize { get => 5_368_709_120.0; } //for better readability (5 GB)
        public static TimeSpan TempFileDeleteTimeSpan { get => TimeSpan.FromMinutes(10); }

        public static Pair<string, string> GetWebControllerAndActionForDetails()
        {
            return new Pair<string, string>("Web", "SharingPage");
        }

        public static Pair<string, string> GetWebControllerAndActionForDownload()
        {
            return new Pair<string, string>("Web", "DownloadPage");
        }
        public static Pair<string, string> GetWebControllerAndActionQRCodeGeneration()
        {
            return new Pair<string, string>("Drive", "GetQRCodeForItem");
        }

        public static string GetPrivateBaseDirectoryForUser(string userId)
        {
            return Path.Combine(GetPrivateBaseDirectory(), userId);
        }
        public static string GetPrivateBaseDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private");
        }
        public static string GetTempFileDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), TempFilePath);
        }
        public static string GetCloudRootPathInDatabase(Guid id)
        {
            return Path.Combine(PrivateRootName, id.ToString());
        }
        public static string GetLogFilesDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ".__Logs__");
        }
        public static string GetLogFilePath()
        {
            return $".__Logs__/{Constants.AppName}-{{Date}}.txt";
        }

        public static string GetSharingRootPathInDatabase(string userName)
        {
            return Path.Combine(PublicRootName, userName);
        }
        public static string GetDefaultFileSavingPath(Guid id)
        {
            return Path.Combine(GetCloudRootPathInDatabase(id), "Documents");
        }

        public static string GetDefaultFileShowingPath()
        {
            return String.Join(PathSeparator, PrivateRootName, "Documents");
        }

        public static string TerminalRedText(string text)
        {
            return $"[[b;red;black]{text}]";
        }
        public static string TerminalGreenText(string text)
        {
            return $"[[b;green;black]{text}]";
        }
        public static string TerminalYellowText(string text)
        {
            return $"[[b;yellow;black]{text}]";
        }
        public static string TerminalWhiteText(string text)
        {
            return $"[[b;white;black]{text}]";
        }
    }
}
