namespace NCloud.Models
{
    public class FileII : CloudRegistrationII
    {
        public override int GetSize()
        {
            return 1; // todo, sablonfuggveny alapjan megcsinalni
        }
        public override bool IsFile()
        {
            return true;
        }
    }
}
