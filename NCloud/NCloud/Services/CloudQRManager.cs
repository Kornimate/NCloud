using QRCoder;
using System.Drawing;

namespace NCloud.Services
{
    /// <summary>
    /// Class to create QR code for web shared files and folders
    /// </summary>
    public static class CloudQRManager
    {
        /// <summary>
        /// Method that generates the QR code and returns a base64 string
        /// </summary>
        /// <param name="url">Url which is the base of the QR code generation (picture points to)</param>
        /// <returns>image source string for HTML img tag with base64 string to be showed on UI</returns>
        public static string GenerateQRCodeString(string? url)
        {
            QRCodeGenerator codeGenerator = new QRCodeGenerator();
            QRCodeData info = codeGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode code = new QRCode(info);
            Bitmap img = code.GetGraphic(30);

            return "data:image/png;base64, " + Convert.ToBase64String((byte[])new ImageConverter().ConvertTo(img, typeof(byte[]))!);
        }
    }
}
