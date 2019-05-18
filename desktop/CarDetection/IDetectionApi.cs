using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace CarDetection
{
    public interface IDetectionApi
    {
        [Put("/api/camera")]
        Task<Camera> PutCameraAsync(Camera camera);

        [Get("/api/feedback")]
        Task<IList<Feedback>> GetAllFeedbacksAsync();
    }
}