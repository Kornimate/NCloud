using Microsoft.AspNetCore.StaticFiles;
using NCloud.ConstantData;

namespace NCloud.Services
{
    /// <summary>
    /// Class to manage MIME types for downloads
    /// </summary>
    public static class CloudMimeTypeManager
    {
        /// <summary>
        /// Static method to get file MIME type
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>The MIME type of file or default MIME type</returns>
        public static string GetMimeType(string fileName)
        {
            string mimeType = Constants.DefaultMimeType;

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out string? possibleMimeType))
            {
                return mimeType;
            }

            return possibleMimeType;
        }
    }
}
