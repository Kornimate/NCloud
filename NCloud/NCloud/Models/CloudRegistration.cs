using NCloud.Models;

namespace NCloud.Models
{
    public abstract class CloudRegistration : Entry
    {
        public static CloudRegistration? CreateRegistration(Entry x)
        {
            if (x.Type == EntryType.FOLDER)
            {
                return new Folder();
            }
            else if (x.Type == EntryType.FILE)
            {
                return new File();
            }
            else
            {
                return null;
            }
        }

        public abstract int GetSize();
    }
}
