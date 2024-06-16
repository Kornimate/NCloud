using NCloud.Models;
using System.Diagnostics;

namespace NCloud.Services
{
    public static class CloudSizeManager
    {
        /// <summary>
        /// Method to convert bytes into readable format (if reaches a bigger size then it is showed in that e.g kb -> mb)
        /// </summary>
        /// <param name="bytes">bytes in double (or long due to auto conversion)</param>
        /// <returns>The readable number in string, rounded to two decimals</returns>
        public static string ConvertToReadableSize(double bytes)
        {
            const long kb = 1024;
            const long mb = 1024 * kb;
            const long gb = 1024 * mb;

            double result;
            string unit;

            switch (bytes)
            {
                case >= gb:
                    result = Math.Round(bytes / gb, 2, MidpointRounding.ToZero);
                    unit = "GB";
                    break;
                case >= mb:
                    result = Math.Round(bytes / mb, 2, MidpointRounding.ToZero);
                    unit = "MB";
                    break;
                case >= kb:
                    result = Math.Round(bytes / kb, 2, MidpointRounding.ToZero);
                    unit = "KB";
                    break;
                default:
                    result = bytes;
                    unit = $"B";
                    break;
            }

            return $"{result:F2} {unit}";
        }

        public static string ConvertToReadableSize(SpaceSizes size)
        {
            return size switch
            {
                SpaceSizes.GB1 => "1 GB",
                SpaceSizes.GB2 => "2 GB",
                SpaceSizes.GB5 => "5 GB",
                SpaceSizes.GB10 => "10 GB",
                SpaceSizes.GB20 => "20 GB",
                SpaceSizes.GB50 => "50 GB",
                SpaceSizes.GB100 => "100 GB",
                _ => "0 GB",
            };
        }
    }
}
