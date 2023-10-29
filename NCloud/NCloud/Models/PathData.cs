using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    public class PathData
    {
        private const string ROOTNAME = "@CLOUDROOT";
        private const string SEPARATOR = "/";

        public string CurrentDirectory { get; private set; }
        public List<string> PreviousDirectories { get; private set; }
        public string CurrentPath { get; private set; }
        public string CurrentPathShow { get; private set; }

        [JsonConstructor]
        public PathData(string currentDirectory, List<string> previousDirectories, string currentPath, string currentPathShow)
        {
            CurrentDirectory = currentDirectory;
            PreviousDirectories = previousDirectories;
            CurrentPath = currentPath;
            CurrentPathShow = currentPathShow;
        }
        public PathData()
        {
            PreviousDirectories = new List<string>();
            CurrentDirectory = String.Empty;
            CurrentPath = ROOTNAME;
            CurrentPathShow = ROOTNAME;
        }

        public void SetDefaultPathData(string id)
        {
            CurrentPath = Path.Combine(CurrentPath, id);
            PreviousDirectories.Clear();
            PreviousDirectories.Add(ROOTNAME);
            PreviousDirectories.Add(id);
        }

        public string SetFolder(string? folderName)
        {
            string currentPath = String.Empty;
            if (folderName == null)
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
            PreviousDirectories.RemoveAt(PreviousDirectories.Count - 1);
            string? folder = PreviousDirectories.Last();
            CurrentDirectory = folder;
            CurrentPath = Path.Combine(PreviousDirectories.ToArray());
            string end = String.Join(SEPARATOR, PreviousDirectories.Skip(2).ToArray());
            if (end == String.Empty)
            {
                CurrentPathShow = ROOTNAME;
            }
            else
            {
                CurrentPathShow = String.Join(SEPARATOR, ROOTNAME,end);
            }
            return folder;
        }
    }
}
