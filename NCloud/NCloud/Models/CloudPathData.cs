using NCloud.ConstantData;
using NCloud.Models.Extensions;
using NCloud.Services;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
    /// <summary>
    /// Container class to save information into session about current state and cloud clipboard
    /// </summary>
    public class CloudPathData
    {
        public List<string> PreviousDirectories { get; private set; }
        public string CurrentPath { get; private set; }
        public string CurrentPathShow { get; private set; }
        public string ClipBoard { get; private set; }

        public bool CanGoBack { get => PreviousDirectories.Count > 2; }

        [JsonConstructor]
        public CloudPathData(List<string> previousDirectories, string currentPath, string currentPathShow, string clipBoard)
        {
            PreviousDirectories = previousDirectories;
            CurrentPath = currentPath;
            CurrentPathShow = currentPathShow;
            ClipBoard = clipBoard;
        }
        public CloudPathData()
        {
            PreviousDirectories = new List<string>();
            CurrentPath = Constants.PrivateRootName;
            CurrentPathShow = Constants.PrivateRootName;
            ClipBoard = String.Empty;
        }

        /// <summary>
        /// Setter method to set default data for user
        /// </summary>
        /// <param name="id">Id of user (from database)</param>
        public void SetDefaultPathData(string? id)
        {
            if (id is null) return;

            CurrentPath = Constants.PrivateRootName;
            CurrentPath = Path.Combine(CurrentPath, id);
            CurrentPathShow = Constants.PrivateRootName;
            PreviousDirectories.Clear();
            PreviousDirectories.Add(Constants.PrivateRootName);
            PreviousDirectories.Add(id);
        }

        /// <summary>
        /// Method to experiment if folder with current state exists
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns></returns>
        public string TrySetFolder(string? folderName)
        {
            if (String.IsNullOrWhiteSpace(folderName))
                return String.Empty;

            return Path.Combine(CurrentPath, folderName);
        }

        /// <summary>
        /// Setter method to set folder and add to current state creating a new current state
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns>The new current state with folder in it</returns>
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
        /// Method to create new current state by going backwards
        /// </summary>
        /// <returns>Returns the earlier state last folder</returns>
        public string? RemoveFolderFromPrevDirs()
        {
            if (PreviousDirectories.Count > 2)
            {
                PreviousDirectories.RemoveAt(PreviousDirectories.Count - 1);

                string? folder = PreviousDirectories.Last();

                CurrentPath = Path.Combine(PreviousDirectories.ToArray());

                string end = String.Join(Constants.PathSeparator, PreviousDirectories.Skip(2).ToArray());

                if (end == String.Empty)
                {
                    CurrentPathShow = Constants.PrivateRootName;
                }
                else
                {
                    CurrentPathShow = String.Join(Constants.PathSeparator, Constants.PrivateRootName, end);
                }

                return folder;
            }

            return null;
        }

        /// <summary>
        /// Setter method to set cloud clipboard data 
        /// </summary>
        /// <param name="cloudPath">Path to file or folder in app</param>
        /// <param name="isFile">Boolean value indicating if item stored is file or folder</param>
        public void SetClipBoardData(string cloudPath, bool isFile)
        {
            ClipBoard = $"{(isFile ? Constants.SelectedFileStarterSymbol : Constants.SelectedFolderStarterSymbol)}{Constants.FileNameDelimiter}{new string(cloudPath)}"; //copy data
        }

        /// <summary>
        /// Getter method to get data from cloud clipboard
        /// </summary>
        /// <returns>The Created CloudRegistration (CloudFile or CloudFolder)</returns>
        /// <exception cref="MissingMemberException">Throws this exception if</exception>
        public CloudRegistration? GetClipBoardData()
        {
            if (String.IsNullOrEmpty(ClipBoard))
            {
                throw new MissingMemberException("No data in cloud clipboard");
            }

            return CloudRegistration.RegistrationPathFactory(ClipBoard); //Creates the file or folder wrapping object
        }

        /// <summary>
        /// Setter method to set current state to given path
        /// </summary>
        /// <param name="path">Path in app</param>
        public void SetPath(string path)
        {
            CurrentPath = path;
            PreviousDirectories = CurrentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            CurrentPathShow = CurrentPath.Slice(Constants.PrivateRootName.Length, Constants.PrivateRootName.Length + Constants.GuidLength + 1).Replace(Path.DirectorySeparatorChar, Constants.PathSeparator);
        }

        /// <summary>
        /// Method to add user info to ablsoute path (retrieved from command line)
        /// </summary>
        /// <param name="path">The absolute path in app</param>
        /// <returns>The new path if operation successful, otherwise empty string</returns>
        public string AddUserInfoToAbsolutePath(string path)
        {
            if (!path.StartsWith(Constants.PrivateRootName))
                return String.Empty;

            int ind = path.IndexOf(Path.DirectorySeparatorChar);

            try
            {
                if (ind == -1)
                    return Path.Combine(path, PreviousDirectories[1]);
                else
                    return Path.Combine(PreviousDirectories[0], PreviousDirectories[1], path[(ind + 1)..]);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
