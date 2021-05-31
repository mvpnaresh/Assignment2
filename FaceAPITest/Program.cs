using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FaceAPITest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var imagePath = @"SampleImage\DSC_1887.JPG";
            Guid orderid = Guid.NewGuid();
            var urlAddress = $"http://localhost:6000/api/faces?orderId={orderid}" ;
            ImageUtility imageUtility = new();
            var bytes = imageUtility.ConvertToBytes(imagePath);
            List<byte[]> faceList = null;
            var byteContent = new ByteArrayContent(bytes);

            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
           
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using var client = new HttpClient(httpClientHandler);
                
                using var response = await client.PostAsync(urlAddress, byteContent);
                string apiResponse = await response.Content.ReadAsStringAsync();
                faceList = JsonConvert.DeserializeObject<List<byte[]>>(apiResponse);
            }


            if (faceList.Count > 0)
            {
                for (int i = 0; i < faceList.Count; i++)
                {
                    imageUtility.FromBytesToImage(faceList[i], "face" + i);
                }
            }
        }
    }
}
