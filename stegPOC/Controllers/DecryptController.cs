﻿using F5;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace stegPOC.Controllers
{
    public class DecryptController : Controller
    {
        // GET: Decrypt
        public ActionResult Index()
        {
            DirectoryInfo dInfo = new DirectoryInfo(@"C:\cats");


            string password = "banana";

            byte[] image = new byte[0];

            var fullstring = "";

            var files = dInfo.GetFiles().OrderBy(x => x.CreationTime);
            foreach (var file in files)
            {
                using (var stream = new MemoryStream())
                {
                    using (JpegExtract extractor = new JpegExtract(stream, password))
                    {
                        extractor.Extract(file.Open(FileMode.Open));

                        stream.Seek(0, SeekOrigin.Begin);
                        var sr = new StreamReader(stream);

                        string bytestring = sr.ReadToEnd();
                        bytestring = sanitiseByteString(bytestring);

                        fullstring += bytestring;

                        byte[] block = Convert.FromBase64String(bytestring); //EncodingHelper.DecodeToByteArray(bytestring);

                        image = combineArrays(image, block);
                    }
                }
            }

           



            using (var outputImage = System.IO.File.Open(@"C:\dog.jpg", FileMode.Create))
            {
                outputImage.Write(image, 0, image.Length);
            }

                return View();
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