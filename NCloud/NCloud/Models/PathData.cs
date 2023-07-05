namespace NCloud.Models
{
    public class PathData
    {
        public int CurrentDirectory { get; set; }
        public List<int> PreviousDirectories { get; set; }
        public string CurrentPath { get; set; }

        public PathData(int parentId)
        {
            PreviousDirectories = new List<int>() { parentId };
            CurrentDirectory = 0;
            CurrentPath = @"@CLOUDROOT";
        }

    }
}
