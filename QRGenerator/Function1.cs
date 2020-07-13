using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using static QRCoder.PayloadGenerator;

namespace QRGenerator
{
    public static class Function1
    {
        [FunctionName("QrFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Receieved Request to generate QR");


            if (!string.IsNullOrEmpty(req.Query["firstname"]))
            {


                ContactData gen = new ContactData(ContactData.ContactOutputType.VCard3, req.Query["firstname"], req.Query["lastname"], req.Query["nickname"]
                    , req.Query["phone"], req.Query["mobilephone"], req.Query["email"], req.Query["country"]);
                string payload = gen.ToString();



                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);



                Bitmap qrCodeImage = qrCode.GetGraphic(20);
              
                return new FileContentResult(ImageToByteArray(qrCodeImage), "image/jpeg");
            }
            else
            {
                return new BadRequestResult();
            }
        }

 
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}
