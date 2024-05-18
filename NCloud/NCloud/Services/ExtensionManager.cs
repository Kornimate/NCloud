using NCloud.ConstantData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace NCloud.Services
{
    /// <summary>
    /// Class to get extension data from resources (json files)
    /// </summary>
    public static class ExtensionManager
    {
        /// <summary>
        /// Static method to get coding extension data
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="extensionData">out parameter to specify extension data retrieved from resources</param>
        /// <returns>Boolean value if extension is supported coding extension</returns>
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

        /// <summary>
        /// Static method to get coding extension data
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="extensionData">out parameter to specify extension data retrieved from resources</param>
        /// <returns>Boolean value if extension is supported etxtd document extension</returns>
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

        /// <summary>
        /// Getter method for coding extension resource file content
        /// </summary>
        /// <returns>List of coding extension in string format</returns>
        public static Task<List<string>> GetCodingExtensions()
        {
            return Task.FromResult<List<string>>(JObject.Parse(File.ReadAllText(Constants.CodingExtensionsFilePath)).Properties().Select(x => x.Name).OrderBy(x => x).ToList());
        }

        /// <summary>
        /// Getter method for text document extension resource file content
        /// </summary>
        /// <returns>List of text document extension in string format</returns>
        public static Task<List<string>> GetTextDocumentExtensions()
        {
            return Task.FromResult<List<string>>(JObject.Parse(File.ReadAllText(Constants.TextDocumentExtensionsFilePath)).Properties().Select(x => x.Name).OrderBy(x => x).ToList());
        }
    }
}
