namespace NCloud.Models.Extensions
{
    /// <summary>
    /// String extension for slice method to slice a string out of another and return the sorroundings
    /// </summary>
    public static class StringExtension
    {
        public static string Slice(this string value, int start, int end)
        {
            return value[..start] + value[end..];
        }
    }
}
