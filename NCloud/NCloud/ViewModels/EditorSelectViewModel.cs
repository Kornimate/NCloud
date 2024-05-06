namespace NCloud.ViewModels
{
    public class EditorSelectViewModel
    {
        public string FileName { get; set; }
        public string? Path { get; set; }
        public string ExtensionDataCoding { get; set; }
        public string ExtensionDataTextDocument { get; set; }
        public string? RedirectData { get; set; }

        public EditorSelectViewModel(string fileName, string? path, string extensionDataCoding, string extensionDataTextDocument, string? redirectData)
        {
            FileName = fileName;
            Path = path;
            ExtensionDataCoding = extensionDataCoding;
            ExtensionDataTextDocument = extensionDataTextDocument;
            RedirectData = redirectData;
        }
    }
}
