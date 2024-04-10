using NCloud.ConstantData;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class SharedPathData
    {
        public string CurrentDirectory { get; private set; }
        public List<string> PreviousDirectories { get; private set; }
        public string CurrentPath { get; private set; }
        public string CurrentPathShow { get; private set; }

        [JsonConstructor]
        public SharedPathData(string currentDirectory, List<string> previousDirectories, string currentPath, string currentPathShow)
        {
            CurrentDirectory = currentDirectory;
            PreviousDirectories = previousDirectories;
            CurrentPath = currentPath;
            CurrentPathShow = currentPathShow;
        }
        public SharedPathData()
        {
            PreviousDirectories = new List<string>() { Constants.PublicRootName };
            CurrentDirectory = String.Empty;
            CurrentPath = Constants.PublicRootName;
            CurrentPathShow = Constants.PublicRootName;
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
                CurrentPathShow += Constants.PathSeparator + folderName;
            }
            
            return currentPath;
        }

        public string? RemoveFolderFromPrevDirs()
        {
            string? folder = Constants.PublicRootName;
            
            if (PreviousDirectories.Count > 1)
            {
                PreviousDirectories.RemoveAt(PreviousDirectories.Count - 1);
                
                folder = PreviousDirectories.Last();
            }
            
            CurrentDirectory = folder;
            CurrentPath = Path.Combine(PreviousDirectories.ToArray());
            CurrentPathShow = String.Join(Constants.PathSeparator, PreviousDirectories.ToArray());
            
            return folder;
        }
    }
}
