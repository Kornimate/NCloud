using NCloud.Models;

namespace NCloud.Models
{
    public abstract class CloudRegistrationII : Entry
    {
        public string? IconPath { get; private set; }
        public static CloudRegistrationII? CreateRegistration(Entry x)
        {
            if (x.Type == EntryType.FOLDER)
            {
                return new FolderII()
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
                return new FileII()
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

        public virtual bool IsFile()
        {
            return false;
        }
        public virtual bool IsFolder()
        {
            return false;
        }
    }
}
