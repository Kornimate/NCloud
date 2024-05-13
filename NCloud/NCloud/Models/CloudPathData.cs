using NCloud.ConstantData;
using NCloud.Models.Extensions;
using System.Security.Claims;
using System.Text;
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
            CurrentPathShow = Constants.PrivateRootName;
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

        public async Task SetPath(string path, string pathStart)
        {
            CurrentPath = await GetFoldersRealName(new string(path), pathStart);
            PreviousDirectories = CurrentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            CurrentPathShow = CurrentPath.Slice(Constants.PrivateRootName.Length, Constants.PrivateRootName.Length + Constants.GuidLength + 1).Replace(Path.DirectorySeparatorChar, Constants.PathSeparator);
        }

        private Task<string> GetFoldersRealName(string path, string pathStart)
        {
            string[] paths = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            if (paths.Length < 2)
                throw new InvalidDataException("error while adjusting path");

            string[] realNames = new string[paths.Length - 1];

            realNames[0] = paths[1];

            for (int i = 2; i < paths.Length; i++)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(pathStart, Path.Combine(realNames[..(i - 1)])));

                if (dirInfo.Exists)
                {
                    realNames[i - 1] = new string(dirInfo.GetDirectories().First(x => x.Name.ToLower() == paths[i].ToLower()).Name);
                }
            }

            return Task.FromResult<string>(Path.Combine(Constants.PrivateRootName, Path.Combine(realNames)));
        }

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
