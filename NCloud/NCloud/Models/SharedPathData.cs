using NCloud.ConstantData;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    /// <summary>
    /// Class to store information about shared current state in session
    /// </summary>
    public class SharedPathData
    {
        public List<string> PreviousDirectories { get; private set; }
        public string CurrentPath { get; private set; }
        public string CurrentPathShow { get; private set; }

        [JsonConstructor]
        public SharedPathData(List<string> previousDirectories, string currentPath, string currentPathShow)
        {
            PreviousDirectories = previousDirectories;
            CurrentPath = currentPath;
            CurrentPathShow = currentPathShow;
        }
        public SharedPathData()
        {
            PreviousDirectories = new List<string>() { Constants.PublicRootName };
            CurrentPath = Constants.PublicRootName;
            CurrentPathShow = Constants.PublicRootName;
        }

        /// <summary>
        /// Setter method to set current sharing state
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns>The new current state path</returns>
        public string SetFolder(string? folderName)
        {
            string currentPath = String.Empty;
            
            if (String.IsNullOrWhiteSpace(folderName))
            {
                currentPath = CurrentPath;
            }
            else
            {
                currentPath = Path.Combine(CurrentPath, folderName);
                
                PreviousDirectories.Add(folderName);
                CurrentPath = $@"{currentPath}"; //for security reasons
                CurrentPathShow += Constants.PathSeparator + folderName;
            }
            
            return currentPath;
        }

        /// <summary>
        /// Method to create new sharing current state by going backwards
        /// </summary>
        /// <returns>The new current state path</returns>
        public string? RemoveFolderFromPrevDirs()
        {
            string? folder = Constants.PublicRootName;
            
            if (PreviousDirectories.Count > 1)
            {
                PreviousDirectories.RemoveAt(PreviousDirectories.Count - 1);
                
                folder = PreviousDirectories.Last();
            }
            
            CurrentPath = Path.Combine(PreviousDirectories.ToArray());
            CurrentPathShow = String.Join(Constants.PathSeparator, PreviousDirectories.ToArray());
            
            return folder;
        }
    }
}
