using F5;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace stegPOC.Controllers
{
    public class DecryptController : Controller
    {
        public async Task<ActionResult> Index(string albumId)
        {
            ViewBag.albumId = albumId;
            return View("SetEncKey");
        }

        public async Task<ActionResult> Decrypt(string albumId, string encryptionKey)
        {
            ViewBag.albumId = albumId;
            ViewBag.encryptionKey = encryptionKey;
            return View();
        }
        // GET: Decrypt
        public async Task<ActionResult> GetImage(string albumId, string encryptionKey)
        {
            DirectoryInfo dInfo = new DirectoryInfo(@"C:\cats");

            var urlList = await imgurConnector.getPhotoIdsFromAlbum(albumId);
            var otherlist = imgurConnector.idList;

            byte[] image = new byte[0];

            var fullstring = "";

            var files = dInfo.GetFiles().OrderBy(x => x.CreationTime);
            
            foreach (string url in urlList)
            {
                using (var client = new HttpClient())
                {
                    var imageResult = await client.GetAsync(url);

                    using (MemoryStream inputStream = new MemoryStream())
                    {
                        await imageResult.Content.CopyToAsync(inputStream);
                        inputStream.Seek(0, SeekOrigin.Begin);

                        using (var outputStream = new MemoryStream())
                        {
                            using (JpegExtract extractor = new JpegExtract(outputStream, encryptionKey))
                            {
                                extractor.Extract(inputStream);

                                outputStream.Seek(0, SeekOrigin.Begin);
                                var sr = new StreamReader(outputStream);

                                string bytestring = sr.ReadToEnd();
                                bytestring = sanitiseByteString(bytestring);

                                fullstring += bytestring;
                                try
                                {
                                    byte[] block = Convert.FromBase64String(bytestring); //EncodingHelper.DecodeToByteArray(bytestring);

                                    image = combineArrays(image, block);
                                }
                                catch(Exception e)
                                {

                                }
                            }
                        }
                    }
                }
            }              

            /*
            using (var outputImage = System.IO.File.Open(@"C:\dog.jpg", FileMode.Create))
            {
                outputImage.Write(image, 0, image.Length);
            }
            */

            return File(image, "image/jpeg");
        }

        public static byte[] combineArrays(byte[] a, byte[] b)
        {
            byte[] returnArray = new byte[a.Length + b.Length];

            int counter = 0;

            while(counter < a.Length)
            {
                returnArray[counter] = a[counter];
                counter++;
            }

            while(counter - a.Length < b.Length)
            {
                returnArray[counter] = b[counter - a.Length];
                counter++;
            }

            return returnArray;
        }

        public static string sanitiseByteString(string input)
        {
            if (input.Contains("="))
            {
                input = input.Split('=')[0];
            }

            while (input.Length % 4 > 0)
            {
                input += "=";
            }

            return input;
        }
    }
}