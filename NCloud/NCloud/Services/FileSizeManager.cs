namespace NCloud.Services
{
    public static class FileSizeManager
    {
        /// <summary>
        /// Method to convert bytes into readable format (if reaches a bigger size then it is showed in that e.g kb -> mb)
        /// </summary>
        /// <param name="bytes">bytes in double (or long due to auto conversion)</param>
        /// <returns>The readable number in string, rounded to two decimals</returns>
        public static string ConvertToReadableSize(double bytes)
        {
            {
                const long kb = 1024;
                const long mb = 1024 * kb;
                const long gb = 1024 * mb;

                double result;
                string unit;

                switch (bytes)
                {
                    case >= gb:
                        result = bytes / gb;
                        unit = "GB";
                        break;
                    case >= mb:
                        result = bytes / mb;
                        unit = "MB";
                        break;
                    case >= kb:
                        result = bytes / kb;
                        unit = "KB";
                        break;
                    default:
                        result = bytes;
                        unit = $"B";
                        break;
                }

                return $"{result:F2} {unit}";
            }
        }
    }
}
