using NCloud.ConstantData;
using QRCoder;
using System.Drawing;

namespace NCloud.Services
{
    public static class CloudQRManager
    {
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
