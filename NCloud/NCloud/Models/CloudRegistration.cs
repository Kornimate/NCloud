using NCloud.Models;

namespace NCloud.Models
{
    public abstract class CloudRegistration : Entry
    {
        public string? IconPath { get; private set; }
        public static CloudRegistration? CreateRegistration(Entry x)
        {
            if (x.Type == EntryType.FOLDER)
            {
                return new Folder()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Size = x.Size,
                    ParentId = x.ParentId,
                    Type = x.Type,
                    CreatedDate = x.CreatedDate,
                    IsVisibleForEveryOne = x.IsVisibleForEveryOne,
                    CreatedBy = x.CreatedBy,
                    IconPath = ImageLoader.Load()
                };
            }
            else if (x.Type == EntryType.FILE)
            {
                return new File()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Size = x.Size,
                    ParentId = x.ParentId,
                    Type = x.Type,
                    CreatedDate = x.CreatedDate,
                    IsVisibleForEveryOne = x.IsVisibleForEveryOne,
                    CreatedBy = x.CreatedBy,
                    IconPath = ImageLoader.Load(x.Name)
                };
            }
            else
            {
                return null;
            }
        }

        public abstract int GetSize();
    }
}
