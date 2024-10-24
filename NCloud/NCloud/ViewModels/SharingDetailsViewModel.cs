﻿using NCloud.Models;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Sharing Details action method
    /// </summary>
    public class SharingDetailsViewModel
    {
        public List<CloudFile> Files { get; set; }
        public List<CloudFolder> Folders { get; set; }
        public string CurrentPath { get; set; }
        public bool Owner { get; set; } = false;
        public SharingDetailsViewModel(List<CloudFile> files, List<CloudFolder> folders, string currentPath, bool owner)
        {
            Files = files;
            Folders = folders;
            CurrentPath = currentPath;
            Owner = owner;
        }
    }
}
