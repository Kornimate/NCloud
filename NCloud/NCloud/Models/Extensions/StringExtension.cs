namespace NCloud.Models.Extensions
{
    public static class StringExtension
    {
        public static string Slice(this string value, int start, int end)
        {
            return value[..start] + value[end..];
        }
    }
}
