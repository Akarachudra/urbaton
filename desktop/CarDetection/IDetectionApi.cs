using System.Threading.Tasks;
using Refit;

namespace CarDetection
{
    public interface IDetectionApi
    {
        [Put("/api/camera")]
        Task<Camera> PutCameraAsync(Camera camera);
    }
}