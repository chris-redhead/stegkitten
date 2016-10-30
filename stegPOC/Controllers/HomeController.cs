using F5.James;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace stegPOC.Controllers
{
    public class HomeController : Controller
    {
        private static int hiddenChunkLength = 400;

        [HttpGet]
        // GET: /<controller>/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase evilfile)
        {
            byte[] imageBytes;
            List<byte[]> chunkList = new List<byte[]>();

            using (var fileStream = evilfile.InputStream)
            {
                var length = fileStream.Length;

                int offset = 0;

                while (offset < fileStream.Length)
                {
                    imageBytes = new byte[hiddenChunkLength];
                    //read into array
                    await fileStream.ReadAsync(imageBytes, 0, hiddenChunkLength);

                    chunkList.Add(imageBytes);
                    offset = offset + hiddenChunkLength;
                }
            }

            int counter = 0;
            foreach (var chunk in chunkList)
            {
                Encode(chunk, @"C:\cats\cat" + counter + ".jpg");
                counter++;
            }


            ViewBag.message = "Successfully uploaded";
            return View("Index");
        }

        private static string fullstring = "";

        private void Encode(byte[] chunk,string fileName)
        {
            int quality = 100;

            string plaintext = Convert.ToBase64String(chunk); //EncodingHelper.EncodeToString(chunk);
            fullstring += plaintext;
            string password = "banana";

            using (var s = GenerateStreamFromString(plaintext))
            {

                //hit the endpoint here
                using (Image image = Image.FromStream(GetCleanImage()))
                using (JpegEncoder jpg = new JpegEncoder(image, System.IO.File.OpenWrite(fileName), "", quality))
                {

                    jpg.Compress(s, password);
                }
            }
        }

        public static System.IO.Stream GenerateStreamFromString(string s)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Random ran = new Random();

        public static System.IO.Stream GetCleanImage()
        {

            using (WebClient client = new WebClient())
            {
                return client.OpenRead($"https://placekitten.com/{300 + ran.Next(100)}/{500 + ran.Next(100)}");
            }
        }

    }
}