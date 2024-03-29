﻿using NCloud.Users;

namespace NCloud.Models
{
    public class Entry
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Size { get; set;  }
        public int ParentId { get; set; }
        public EntryType? Type { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsVisibleForEveryOne { get; set; }
        public virtual CloudUser CreatedBy { get; set; } = null!;
    }
}
