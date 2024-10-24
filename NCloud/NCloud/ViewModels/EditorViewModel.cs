﻿namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Editor specific editors action method
    /// </summary>
    public class EditorViewModel
    {
        public string FilePath { get; set; } = String.Empty;
        public string FileExtension { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty!;
        public string ExtensionData { get; set; } = String.Empty;
        public string Redirection { get; set; } = String.Empty;
        public string Encoding { get; set; } = String.Empty;
        public string EncodingName { get; set; } = String.Empty;
    }
}
