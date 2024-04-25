using Microsoft.AspNetCore.StaticFiles;
using NCloud.ConstantData;

namespace NCloud.Services
{
    public static class FormatManager
    {
        public static string GetMimeType(string fileName)
        {
            string mimeType = Constants.DefaultMimeType;

            var provider = new FileExtensionContentTypeProvider();

            if(!provider.TryGetContentType(fileName, out string? possibleMimeType))
            {
                return mimeType;
            }

            return possibleMimeType;
        }
    }
}
