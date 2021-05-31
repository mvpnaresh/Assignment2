using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacesAPI.Interfaces
{
    public interface IFaceRecognitionApi
    {
        Task<List<byte[]>> GetFaces(byte[] image);
    }
}
