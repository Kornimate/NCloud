namespace NCloud.Services
{
    public static class FileSizeManager
    {
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
                        unit = $"byte{(result > 1 ? "s" : "")}";
                        break;
                }

                return $"{result:F2} {unit}";
            }
        }
    }
}
