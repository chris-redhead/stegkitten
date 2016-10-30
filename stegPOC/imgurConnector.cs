using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace stegPOC
{
    public class imgurConnector
    {
        public static List<string> idList = new List<string>();

        public static string clientId = "ce6f4ba18dfae46";

        public static async Task<List<string>> getPhotoIdsFromAlbum(string albumId)
        {
            using (var client = new HttpClient())
            {
                string url = "https://api.imgur.com/3/album/" + albumId;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);
                var imageResult = await client.GetAsync(url);

                using (MemoryStream inputStream = new MemoryStream())
                {
                    await imageResult.Content.CopyToAsync(inputStream);
                    inputStream.Seek(0, SeekOrigin.Begin);

                    var sr = new StreamReader(inputStream);

                    string jsonstring = sr.ReadToEnd();

                    dynamic resultObj = JsonConvert.DeserializeObject(jsonstring);

                    List<string> returnList = new List<string>();

                    List<imageRecord> records = new List<imageRecord>();

                    foreach (var imagerecord in resultObj.data.images)
                    {
                        imageRecord rec = new imageRecord();
                        rec.date = Convert.ToInt64(imagerecord.datetime);
                        rec.link = imagerecord.link.ToString();
                        records.Add(rec);
                        //returnList.Add(imagerecord.link.ToString());
                    }
                    //returnList.Reverse();

                    returnList = records.OrderBy(x => x.date).Select(x => x.link).ToList();

                    return returnList;
                }
            }
        }

        public class imageRecord
        {
            public long date { get; set; }
            public string link { get; set; }
        }

        public static async Task<string> CreateAlbum(List<string> imageIDs)
        {
            using (var client = new HttpClient())
            {
                string idstring = "";

                foreach (var id in imageIDs)
                {
                    idstring += "\"" + id + "\",";
                }

                idstring = idstring.TrimEnd(new char[] { ',' });

                string contentString = "{\"deletehashes\":[" + idstring + "]}";

                client.BaseAddress = new Uri("https://api.imgur.com");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);
                HttpContent content = new StringContent(contentString, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.imgur.com/3/album", content);

                var responseText = await response.Content.ReadAsStringAsync();

                dynamic responseObj = JsonConvert.DeserializeObject(responseText);

                return responseObj.data.id;
            }
        }

        public static async Task<string> PostImage(Stream image)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.imgur.com");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", clientId);
                MultipartFormDataContent form = new MultipartFormDataContent();
                HttpContent content = new StringContent("fileToUpload");
                form.Add(content, "fileToUpload");
                content = new StreamContent(image);
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "image",
                    FileName = "image"
                };
                form.Add(content);


                var response = await client.PostAsync("https://api.imgur.com/3/image", content);

                if (response.IsSuccessStatusCode)
                {

                    var responseText = await response.Content.ReadAsStringAsync();

                    dynamic responseObj = JsonConvert.DeserializeObject(responseText);
                    idList.Add(responseObj.data.link.ToString());
                    return responseObj.data.deletehash;
                }
                else
                {
                    throw new Exception("Oh dear oh dear");
                }
            }
        }
    }
}