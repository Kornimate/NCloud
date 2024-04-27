using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NCloud.Models
{
    public class SharedFile : Sharedregistration
    {
        public override bool IsFile() {  return true; }
    }
}
