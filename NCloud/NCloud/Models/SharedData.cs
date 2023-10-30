using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class SharedData
    {
        public static string ROOTNAME { get => "@SHAREDROOT"; }
        private const string SEPARATOR = "/";

        public string CurrentDirectory { get; private set; }
        public List<string> PreviousDirectories { get; private set; }
        public string CurrentPath { get; private set; }
        public string CurrentPathShow { get; private set; }

        [JsonConstructor]
        public SharedData(string currentDirectory, List<string> previousDirectories, string currentPath, string currentPathShow)
        {
            CurrentDirectory = currentDirectory;
            PreviousDirectories = previousDirectories;
            CurrentPath = currentPath;
            CurrentPathShow = currentPathShow;
        }
        public SharedData()
        {
            PreviousDirectories = new List<string>() { ROOTNAME };
            CurrentDirectory = String.Empty;
            CurrentPath = ROOTNAME;
            CurrentPathShow = ROOTNAME;
        }
        public string? TrySetFolder(string? folderName)
        {
            if (folderName is null) return null;
            return Path.Combine(CurrentPath, folderName);
        }

        public string SetFolder(string? folderName)
        {
            string currentPath = String.Empty;
            if (folderName == null || folderName == String.Empty)
            {
                currentPath = CurrentPath;
            }
            else
            {
                currentPath = Path.Combine(CurrentPath, folderName);
                PreviousDirectories.Add(folderName);
                CurrentPath = $@"{currentPath}"; //for security reasons
                CurrentDirectory = folderName!;
                CurrentPathShow += SEPARATOR + folderName;
            }
            return currentPath;
        }

        public string? RemoveFolderFromPrevDirs()
        {
            string? folder = ROOTNAME;
            if (PreviousDirectories.Count > 1)
            {
                PreviousDirectories.RemoveAt(PreviousDirectories.Count - 1);
                folder = PreviousDirectories.Last();
            }
            CurrentDirectory = folder;
            CurrentPath = Path.Combine(PreviousDirectories.ToArray());
            CurrentPathShow = String.Join(SEPARATOR, PreviousDirectories.ToArray());
            return folder;
        }
    }
}
