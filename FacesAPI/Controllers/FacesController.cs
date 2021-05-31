using FacesAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FacesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacesController : ControllerBase
    {
        private readonly IFaceRecognitionApi _faceRecognitionApi;

        public FacesController(IFaceRecognitionApi faceRecognitionApi)
        {
            _faceRecognitionApi = faceRecognitionApi;
        }

        [HttpPost]
        public async Task<Tuple<List<byte[]>, Guid>> ReadFaces(Guid orderId)
        {
            using (var ms = new MemoryStream(2048))
            {
                await Request.Body.CopyToAsync(ms);
                byte[] imageBytes = ms.ToArray();

                var facesCropped = await _faceRecognitionApi.GetFaces(imageBytes);

                return new Tuple<List<byte[]>, Guid>(facesCropped, orderId);
            }
        }
    }
}
