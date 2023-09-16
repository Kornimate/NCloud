namespace NCloud.Models
{
    public class PathData
    {
        public string CurrentDirectory { get; set; }
        public List<string> PreviousDirectories { get; set; }
        public string CurrentPath { get; set; }

        public PathData(string id)
        {
            PreviousDirectories = new List<string>();
            CurrentDirectory = String.Empty;
            CurrentPath = Path.Combine(@"@CLOUDROOT","//",id);
        }

    }
}
