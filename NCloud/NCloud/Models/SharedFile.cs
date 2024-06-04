namespace NCloud.Models
{
    /// <summary>
    /// Class to create database scheme for shared files
    /// </summary>
    public class SharedFile : Sharedregistration
    {
        public override bool IsFile() { return true; }
    }
}
