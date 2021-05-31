using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacesAPI.Interfaces.Implementation.OpenCV2
{
    public class OpenCV2FaceRecognitionApi : IFaceRecognitionApi
    {
        public async Task<List<byte[]>> GetFaces(byte[] image)
        {
            var faceList = new List<byte[]>();
            Task t = Task.Run(() =>
            {
               Mat src = Cv2.ImDecode(image, ImreadModes.Color);
               src.SaveImage("Image.jpg", new ImageEncodingParam(ImwriteFlags.JpegProgressive, 255));
               var outputPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
               var file = Path.Combine(outputPath, "CascadeFile", "haarcascade_frontalface_default.xml");
               var faceCascade = new CascadeClassifier();
               faceCascade.Load(file);

               var faces = faceCascade.DetectMultiScale(src, 1.1, 6, HaarDetectionTypes.DoRoughSearch, new Size(60, 60));
              
               int j = 0;
               foreach (var rect in faces)
               {
                   var faceImage = new Mat(src, rect);
                   faceList.Add(faceImage.ToBytes(".jpg"));
                   faceImage.SaveImage("face" + j + ".jpg", new ImageEncodingParam(ImwriteFlags.JpegProgressive, 255));
                   j++;
               }
            });
            await t;
            return faceList;

        }


    }
}
