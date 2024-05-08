using NCloud.ConstantData;
using NCloud.Models.Extensions;
using System.Text.Json.Serialization;

namespace NCloud.Models
{
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

        public void SetDefaultPathData(string? id)
        {
            if (id is null) return;

            CurrentPath = Constants.PrivateRootName;
            CurrentPath = Path.Combine(CurrentPath, id);
            PreviousDirectories.Clear();
            PreviousDirectories.Add(Constants.PrivateRootName);
            PreviousDirectories.Add(id);
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
                CurrentPathShow += Constants.PathSeparator + folderName;
            }

            return currentPath;
        }

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

        public void SetClipBoardData(string text, bool isFile)
        {
            ClipBoard = $"{(isFile ? Constants.SelectedFileStarterSymbol : Constants.SelectedFolderStarterSymbol)}{Constants.FileNameDelimiter}{new string(text)}"; //copy data
        }

        public CloudRegistration? GetClipBoardData()
        {
            if (ClipBoard is null || ClipBoard == String.Empty)
            {
                throw new MissingMemberException("No data in clipboard");
            }

            return CloudRegistration.RegistrationPathFactory(ClipBoard);
        }

        public void SetPath(string path)
        {
            CurrentPath = path;
            PreviousDirectories = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            CurrentPathShow = path.Slice(Constants.PrivateRootName.Length, Constants.PrivateRootName.Length + Constants.GuidLength + 1).Replace(Path.DirectorySeparatorChar, Constants.PathSeparator);
        }
    }
}
