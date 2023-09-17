namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public CloudRegistration(string? fileName = null)
        {
            IconPath = ImageLoader.Load(fileName);
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
    }
}
