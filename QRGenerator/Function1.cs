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
using MixERP.Net.VCards;
using MixERP.Net.VCards.Models;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using MixERP.Net.VCards.Serializer;
using MixERP.Net.VCards.Extensions;

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
                string payload = GetvCard(req);

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

        private static string GetvCard(HttpRequest req)
        {
            //ContactData gen = new ContactData(ContactData.ContactOutputType.VCard3, req.Query["firstname"], req.Query["lastname"], req.Query["nickname"]
            //    , req.Query["phone"], req.Query["mobilephone"], req.Query["email"], req.Query["country"]);
            //string payload = gen.ToString();
            //return payload;

            //var data = new ContactData(ContactData.ContactOutputType.MeCard)

            var phone = req.Query["phone"].ToString().Trim();



            var mobilephone = req.Query["mobilephone"].ToString().Trim();



            var vcard = new VCard();
            vcard.Version = MixERP.Net.VCards.Types.VCardVersion.V3;
            if (!string.IsNullOrEmpty(req.Query["firstname"])) vcard.FirstName = req.Query["firstname"];
            if (!string.IsNullOrEmpty(req.Query["lastname"])) vcard.LastName = req.Query["lastname"];
            if (!string.IsNullOrEmpty(req.Query["nickname"])) vcard.NickName = req.Query["nickname"];
            if (!string.IsNullOrEmpty(req.Query["phone"])) vcard.Telephones = new List<Telephone>() { (new Telephone() { Number = phone, Type = MixERP.Net.VCards.Types.TelephoneType.Work }) };
            if (!string.IsNullOrEmpty(req.Query["mobilephone"])) vcard.Telephones = new List<Telephone>() { (new Telephone() { Number = mobilephone, Type = MixERP.Net.VCards.Types.TelephoneType.Cell }) };
            if (!string.IsNullOrEmpty(req.Query["email"])) vcard.Emails = new List<Email>() { (new Email() { EmailAddress = req.Query["email"], Type = MixERP.Net.VCards.Types.EmailType.Smtp }) };
            if (!string.IsNullOrEmpty(req.Query["country"])) vcard.Addresses = new List<Address>() { (new Address() { Country = req.Query["country"] }) };
            if (!string.IsNullOrEmpty(req.Query["title"])) vcard.Title = req.Query["title"];
            if (!string.IsNullOrEmpty(req.Query["role"])) vcard.Role = req.Query["role"];
            if (!string.IsNullOrEmpty(req.Query["org"])) vcard.Organization = req.Query["org"];

            

            return vcard.Serialize();



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
