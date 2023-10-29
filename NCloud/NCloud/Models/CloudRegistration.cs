namespace NCloud.Models
{
    public abstract class CloudRegistration
    {
        public string? IconPath { get; set; }
        public bool IsShared { get; set; }
        public CloudRegistration(bool isShared, string? fileName = null)
        {
            IconPath = ImageLoader.Load(fileName);
            IsShared = isShared;
        }

        public abstract bool IsFile();
        public abstract bool IsFolder();
    }
}
