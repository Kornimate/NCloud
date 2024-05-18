using NCloud.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace NCloud.Models
{
    /// <summary>
    /// Class to create database scheme for shared folders
    /// </summary>
    public class SharedFolder :Sharedregistration
    {
        public override bool IsFolder() { return true; }
    }
}
