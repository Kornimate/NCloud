namespace NCloud.Models.Extensions
{
    /// <summary>
    /// String extension for slice method to slice a string out of another and return the sorroundings
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// String extension to slice a substring out of a string and return the surroundings as a single concatenated string
        /// </summary>
        /// <param name="value">String to be sliced</param>
        /// <param name="start">start index of charachter (zero-based)</param>
        /// <param name="end">end index. charachter (zero-based)</param>
        /// <returns>The sliced string</returns>
        public static string Slice(this string value, int start, int end)
        {
            try
            {
                return value[..start] + value[end..];
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
