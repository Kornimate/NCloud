using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCloud.Models
{
    /// <summary>
    /// Class to store login data in database
    /// </summary>
    public class CloudLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("UserId")]
        public virtual CloudUser? User { get; set; }

        public CloudLogin()
        {
            Id = Guid.NewGuid();
            Date = DateTime.UtcNow;
            User = null!;
        }
        public CloudLogin(CloudUser user)
        {
            Id = Guid.NewGuid();
            Date = DateTime.UtcNow;
            User = user;
        }
    }
}
