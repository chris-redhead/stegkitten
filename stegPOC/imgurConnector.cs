using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace stegPOC
{
    public class imgurConnector
    {
        public static string clientId = "ce6f4ba18dfae46";

        public static void PostImage(Stream image)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.
            }
        }
    }
}