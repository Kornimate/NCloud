namespace NCloud.Models
{
    public class Folder : CloudRegistration
    {
        public override int GetSize()
        {
            return 0; // todo, sablonfuggveny alapjan megcsinalni
        }
        public override bool IsFolder()
        {
            return true;
        }
    }
}
