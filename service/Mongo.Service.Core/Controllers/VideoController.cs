using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Mongo.Service.Core.Controllers
{
    public class VideoController : ApiController
    {
        private const int FramesCount = 7;
        private const string BasePath = "C:\\FakeVideo";
        private readonly TimeSpan framesDiff = TimeSpan.FromSeconds(3);
        private static DateTime lastReponseDateTime = DateTime.MinValue;
        private static int currentFrame;

        [HttpGet]
        [Route("api/video/frame")]
        public HttpResponseMessage GetCameraFile()
        {
            var newLastResponseDateTime = DateTime.UtcNow;
            if ((newLastResponseDateTime - lastReponseDateTime).TotalSeconds > framesDiff.TotalSeconds)
            {
                currentFrame = (currentFrame + 1) % FramesCount;
                lastReponseDateTime = newLastResponseDateTime;
            }

            var path = Path.Combine(BasePath, $"{currentFrame}.jpg");
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    result.Content = new ByteArrayContent(ms.ToArray());

                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    return result;
                }
            }
        }
    }
}