using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FacesAPI.Interfaces.Implementation.Azure.Vision.Face
{
    public class AzureCognitiveServiceFaceApi : IFaceRecognitionApi
    {
        private readonly AzureFaceConfiguration _azureFaceConfig;
        public AzureCognitiveServiceFaceApi(IConfiguration configuration)
        {
            var azureEndPoint = configuration["AzureFaceCredentials:AzureEndPoint"];
            var azureSubscriptionKey = configuration["AzureFaceCredentials:AzureSubscriptionKey"];

            _azureFaceConfig = new AzureFaceConfiguration()
            {
                AzureEndPoint = azureEndPoint,
                AzureSubscriptionKey = azureSubscriptionKey
            };
        }

        private static IFaceClient Authenticate(AzureFaceConfiguration azureFaceConfiguration)
        {
            var faceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(azureFaceConfiguration.AzureSubscriptionKey))
                {
                    Endpoint = azureFaceConfiguration.AzureEndPoint
                };
            return faceClient;
        }

        public async Task<List<byte[]>> GetFaces(byte[] image)
        {
            
            var faceList = new List<byte[]>();

            Image img = Image.Load(image);
            img.Save("dummy.jpg");

            IList<DetectedFace> faces;

            try
            {
                var faceClient = Authenticate(_azureFaceConfig);
                var imageStream = new MemoryStream(image);
                faces = await faceClient.Face.DetectWithStreamAsync(imageStream, true, false, null);
                int j = 0;
                foreach (var face in faces)
                {
                    var s = new MemoryStream();
                    var zoom = 1.0;
                    int imageHeight = (int)(face.FaceRectangle.Height / zoom);
                    int imageWidth = (int)(face.FaceRectangle.Width / zoom);
                    int x = (int)face.FaceRectangle.Left;
                    int y = (int)face.FaceRectangle.Top;

                    //img.Clone(ctx => ctx.Crop(new Rectangle(x, y, imageWidth, imageHeight))).Save("face" + j + ".jpg");
                    img.Clone(ctx => ctx.Crop(new Rectangle(x, y, imageWidth, imageHeight))).SaveAsJpeg(s);
                    faceList.Add(s.ToArray());

                    j++;
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);                
            }
            return faceList;
        }
    }
}
