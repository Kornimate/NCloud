﻿using Castle.Core;
using NCloud.Models;

namespace NCloud.ViewModels
{
    public class WebDetailsViewModel
    {
        public List<CloudFile> Files { get; set; }
        public List<CloudFolder> Folders { get; set; }
        public string CurrentPath { get; set; }
        public WebDetailsViewModel(List<CloudFile> files, List<CloudFolder> folders, string currentPath)
        {
            Files = files;
            Folders = folders;
            CurrentPath = currentPath;
        }
    }
}