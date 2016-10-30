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
        private static int hiddenChunkLength = 350;
        public static List<string> urlList = new List<string>();

        [HttpGet]
        // GET: /<controller>/
        public async Task<ActionResult> Index()
        {            
            return View();           
        }

        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase evilfile)
        {
            urlList = new List<string>();

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
                await Encode(chunk, @"C:\cats\cat" + counter + ".jpg");
                counter++;
            }

            //add all images to album
            var albumId = await imgurConnector.CreateAlbum(urlList);
            var url = "https://imgur.com/a/" + albumId;
            ViewBag.albumId = albumId;
            ViewBag.url = url;
            ViewBag.message = true;
            return View("Index");
        }

        private static string fullstring = "";

        private async Task Encode(byte[] chunk,string fileName)
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

                //open file and write to imgur
                var id = await imgurConnector.PostImage(System.IO.File.Open(fileName, System.IO.FileMode.Open));
                urlList.Add(id);
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
            //347-341x461-464 is ok
            using (WebClient client = new WebClient())
            {
                int counterA = ran.Next(3);
                int counterB = ran.Next(3);
                int width = 347 + counterA; //300 + ran.Next(100)
                int height = 461 + counterB; //400 + ran.Next(100)

                return client.OpenRead($"https://placekitten.com/{width}/{height}");
            }
        }

    }
}