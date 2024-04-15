namespace NCloud.Security
{
    public class HashCreator
    {
        public static string EncryptString(string? input)
        {
            if(input is null)
            {
                return String.Empty;
            }

            //TODO: implement encryption
            return input;
        }
        public static string DecryptString(string? input)
        {
            if (input is null)
            {
                return String.Empty;
            }

            //TODO: implement decryption
            return input;
        }
    }
}
