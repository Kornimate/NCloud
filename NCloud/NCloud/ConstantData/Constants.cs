using Castle.Core;
using NCloud.Models;
using NCloud.Users;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCloud.ConstantData
{
    /// <summary>
    /// Class to store constants used application wide
    /// </summary>
    public static class Constants
    {
        private static IConfiguration? config;
        public static IConfiguration Configuration
        {
            get
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                config = builder.Build();
                return config;
            }
        }

        public static CloudBrandingContainer Branding
        {
            get
            {
                if(config is null)
                {
                    
                    return Configuration.GetSection("Branding").Get<CloudBrandingContainer>() ?? new CloudBrandingContainer()
                    {
                        AppName = "NoName"
                    };
                }

                return config.GetSection("Branding").Get<CloudBrandingContainer>() ?? new CloudBrandingContainer()
                {
                    AppName = "NoName"
                };
            }
        }

        public static readonly List<string> SystemFolders = new List<string>()
        {
            "Documents",
            "Music",
            "Pictures",
            "Videos"
        };

        public static readonly List<string> SpecialFolders = new List<string>()
        {
            "archives",
            "documents",
            "favorites",
            "git",
            "ideas",
            "music",
            "network",
            "photos",
            "pictures",
            "secret",
            "svn",
            "trash",
            "videos"
        };

        public const string AdminRoleName = "admin";
        public static string AdminRole { get => AdminRoleName; }
        public static string ErrorResult { get => "#"; }
        public static string NoFileType { get => "notype"; }
        public static string FolderAndFileRegex { get => @"^[0-9a-zA-Z_!%+=.()$\s]+$"; }
        public static string InvalidSearchPattern { get => @"######"; }
        public static string AbsolutePathRegex { get => @"^[0-9a-zA-Z_!%+=()\\$\s@]+$"; }
        public static string RelativePathRegex { get => @"^[0-9a-zA-Z_!%+=.()\\$\s@]+$"; }
        public static string CommandRegex { get => @"^[0-9a-zA-Z_!%+=.()-/$\s""@]+$"; }
        public static string UserRegex { get => @"^[0-9a-zA-Z_!%+=.()/$\s""]+$"; }
        public static string ZipMimeType { get => "application/zip"; }
        public static string DefaultMimeType { get => "application/octet-stream"; }
        public static string DefaultFileName { get => "<invalid-name>"; }
        public static string DateTimeFormat { get => "yyyy'-'MM'-'dd'T'HH'-'mm'-'ss"; }
        public static string TerminalDateTimeFormat { get => "yyyy'-'MM'-'dd' 'HH':'mm"; }
        public static string UnkownFileType { get => "unknown"; }
        public static string UserRole { get => "user"; }
        public static string TextEditor { get => "Text"; }
        public static string CodeEditor { get => "Code"; }
        public static string FileTypePrefix { get => "filetype-"; }
        public static string FolderPrefix { get => "folder-"; }
        public static string WebRootFolderName { get => ".__CloudData__"; }
        public static string IconsBasePath { get => Path.Combine("wwwroot", "utilities"); }
        public static string LogoPath { get => Path.Combine("wwwroot", "utilities", "cloud_logo.png"); }
        public static string TempFolderName { get => "temp"; }
        public static string TempFilePath { get => Path.Combine(".__TempFiles__", TempFolderName); }
        public static string SimpleFolderIcon { get => "/utilities/folder.svg"; }
        public static string PrefixForSpecialFolders { get => "/utilities/folder-"; }
        public static string SuffixForSpecialFolders { get => ".svg"; }
        public static string SmtpProvider { get => "smtp.gmail.com"; }
        public static string PrefixForFiles { get => "/utilities/filetype-"; }
        public static string SuffixForFiles { get => ".svg"; }
        public static string AdminUserName { get => "Admin"; }
        public static string PrivateRootName { get => "@CLOUDROOT"; }
        public static string PublicRootName { get => "@SHAREDROOT"; }
        public static string CompressedArchiveFileType { get => "zip"; }
        public static string NotSelectedResult { get => "false"; }
        public static string CodingExtensionsFilePath { get => "./Services/Resources/coding-extensions.json"; }
        public static string TextDocumentExtensionsFilePath { get => "./Services/Resources/text-document-extensions.json"; }
        public static string TerminalCommandsDataFilePath { get => "./Services/Resources/terminal-commands.json"; }
        public static string ControllerDataSeparator { get => "||"; }
        public static string NotificationCookieKey { get => "Notification"; }
        public static string AppCookieName { get => "CloudCookie"; }
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
        public static int IconSizeFolder { get => 40; }
        public static int IconSizeFile { get => 30; }
        public static double UserSpaceSize { get => 5_368_709_120.0; } //for better readability (5 GB), base storage size for user
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

        public static string GetPrivateCloudDirectoryForUser(CloudUser user)
        {
            if (user is null)
                return String.Empty;

            return Path.Combine(PrivateRootName, user.Id.ToString());
        }

        /// <summary>
        /// static method to get physical path of base directorz of user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>The physical path to user base directory</returns>
        public static string GetPrivateBaseDirectoryForUser(string userId)
        {
            return Path.Combine(GetPrivateBaseDirectory(), userId);
        }

        /// <summary>
        /// Static method to get base directory for stored items
        /// </summary>
        /// <returns>The physical path to stored items</returns>
        public static string GetPrivateBaseDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private");
        }

        /// <summary>
        /// Static method to get path to temp files folder
        /// </summary>
        /// <returns>The physical path to temp files folder</returns>
        public static string GetTempFileDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), TempFilePath);
        }

        /// <summary>
        /// Static method to get path in app to user base directory
        /// </summary>
        /// <param name="id">user as CloudUser</param>
        /// <returns>The path in app</returns>
        public static string GetCloudRootPathInDatabase(Guid id)
        {
            return Path.Combine(PrivateRootName, id.ToString());
        }

        /// <summary>
        /// Static method to get physical path to logs
        /// </summary>
        /// <returns>The physical path to logs</returns>
        public static string GetLogFilesDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ".__Logs__");
        }

        /// <summary>
        /// Static method to get logs files path for logging NuGet
        /// </summary>
        /// <returns>The format string for logging NuGet</returns>
        public static string GetLogFilePath()
        {
            return $".__Logs__/{Branding.AppName}-{{Date}}.txt";
        }

        /// <summary>
        /// Static method to get sharing path root for user
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>The sharing path root for user</returns>
        public static string GetSharingRootPathInDatabase(string userName)
        {
            return Path.Combine(PublicRootName, userName);
        }

        /// <summary>
        /// Static method to get path of saving files from editor (if new file created)
        /// </summary>
        /// <param name="id">Id of user</param>
        /// <returns>The default saving path for new files in app</returns>
        public static string GetDefaultFileSavingPath(Guid id)
        {
            return Path.Combine(GetCloudRootPathInDatabase(id), "Documents");
        }

        /// <summary>
        /// Static method to get default file saving path in showable form
        /// </summary>
        /// <returns>The default file saving path in showable form</returns>
        public static string GetDefaultFileShowingPath()
        {
            return String.Join(PathSeparator, PrivateRootName, "Documents");
        }

        /// <summary>
        /// Static method to print red text in cloud terminal
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        /// <returns>The formatted text</returns>
        public static string TerminalRedText(string text)
        {
            return $"[[b;red;black]{text}]";
        }

        /// <summary>
        /// Static method to print green text in cloud terminal
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        /// <returns>The formatted text</returns>
        public static string TerminalGreenText(string text)
        {
            return $"[[b;green;black]{text}]";
        }

        /// <summary>
        /// Static method to print red yellow in cloud terminal
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        /// <returns>The formatted text</returns>
        public static string TerminalYellowText(string text)
        {
            return $"[[b;yellow;black]{text}]";
        }
        /// <summary>
        /// Static method to print white text in cloud terminal
        /// </summary>
        /// <param name="text">The text to be formatted</param>
        /// <returns>The formatted text</returns>
        public static string TerminalWhiteText(string text)
        {
            return $"[[b;white;black]{text}]";
        }
    }
}
