namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Account Index action method
    /// </summary>
    public class AccountViewModel
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public long MaxSpace { get; set; }
        public long UsedSpace { get; set; }

        public AccountViewModel(string? username, string? fullName, string? email, long maxSpace, long usedSpace)
        {
            UserName = username;
            FullName = fullName;
            Email = email;
            MaxSpace = maxSpace;
            UsedSpace = usedSpace;
        }
    }
}
