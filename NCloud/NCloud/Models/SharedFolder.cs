namespace NCloud.Models
{
    /// <summary>
    /// Class to create database scheme for shared folders
    /// </summary>
    public class SharedFolder : Sharedregistration
    {
        public override bool IsFolder() { return true; }
    }
}
