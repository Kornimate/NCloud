using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NCloud.Models
{
    /// <summary>
    /// Class to create database scheme for shared files
    /// </summary>
    public class SharedFile : Sharedregistration
    {
        public override bool IsFile() {  return true; }
    }
}
