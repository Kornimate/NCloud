using NCloud.ConstantData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace NCloud.Services
{
    public static class ExtensionManager
    {
        public static Task<bool> TryGetFileCodingExtensionData(string fileName, out string extensionData)
        {

            string extension = Path.GetExtension(fileName).TrimStart(Constants.FileExtensionDelimiter).ToLower();

            try
            {
                extensionData = JObject.Parse(File.ReadAllText(Constants.CodingExtensionsFilePath))[extension]?.ToString() ?? String.Empty;

                return Task.FromResult<bool>(extensionData != String.Empty);
            }
            catch (Exception)
            {
                extensionData = String.Empty;

                return Task.FromResult<bool>(false);
            }
        }

        public static Task<bool> TryGetFileTextDocumentExtensionData(string fileName, out string extensionData)
        {

            string extension = Path.GetExtension(fileName).TrimStart(Constants.FileExtensionDelimiter).ToLower();

            try
            {
                extensionData = JObject.Parse(File.ReadAllText(Constants.TextDocumentExtensionsFilePath))[extension]?.ToString() ?? String.Empty;

                return Task.FromResult<bool>(extensionData != String.Empty);
            }
            catch (Exception)
            {
                extensionData = String.Empty;

                return Task.FromResult<bool>(false);
            }
        }

        public static Task<List<string>> GetCodingExtensions()
        {
            return Task.FromResult<List<string>>(JObject.Parse(File.ReadAllText(Constants.CodingExtensionsFilePath)).Properties().Select(x => x.Name).OrderBy(x => x).ToList());
        }

        public static Task<List<string>> GetTextDocumentExtensions()
        {
            return Task.FromResult<List<string>>(JObject.Parse(File.ReadAllText(Constants.TextDocumentExtensionsFilePath)).Properties().Select(x => x.Name).OrderBy(x => x).ToList());
        }
    }
}
