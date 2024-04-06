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

        public static string FolderIcon { get => "/utilities/folder.svg"; }
        public static string PrefixForIcons { get => "/utilities/filetype-"; }
        public static string SuffixForIcons { get => ".svg"; }
        public static string AdminUserName { get => "Admin"; }
        public static string AppName { get => "NCloudDrive"; }
        public static string PrivateRootName { get => "@CLOUDROOT"; }
        public static string PublicRootName { get => "@SHAREDROOT"; }
        public static string CompressedArchiveFileType { get => "zip"; }
        public static string NotSelectedResult { get => "false"; }
        public static char SelectedFileStarterSymbol { get => '_'; }
        public static char SelectedFolderStarterSymbol { get => '&'; }
        public static char FileNameDelimiter { get => '_'; }
        public static char PathSeparator { get => '/'; }
        public static int DistanceToRootFolder { get => 4; }
        public static int EmptyFolderAttributeNumber { get => 16; }
    }
}
