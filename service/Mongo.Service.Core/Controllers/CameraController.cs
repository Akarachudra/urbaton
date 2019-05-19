using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    public class CameraController : ApiController
    {
        private readonly IEntityStorage<Camera> cameraRepository;

        public CameraController(IEntityStorage<Camera> cameraRepository)
        {
            this.cameraRepository = cameraRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<ApiCamera>> GetAllAsync()
        {
            var cameras = await this.cameraRepository.ReadAllAsync().ConfigureAwait(false);
            return cameras.Select(
                x => new ApiCamera
                {
                    Places = x.Places.Select(
                                  p => new ApiPlace
                                  {
                                      Height = p.Height,
                                      Id = p.Id,
                                      Width = p.Width,
                                      X = p.X,
                                      Y = p.Y
                                  })
                              .ToList(),
                    Number = x.Number,
                    Description = x.Description,
                    Url = x.Url
                });
        }

        [HttpGet]
        [Route("api/camera/file/{id}")]
        public HttpResponseMessage GetCameraFile(int id)
        {
            var path = $"{id}.jpg";
            if (!File.Exists(path))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            lock (Cache.Locker)
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        result.Content = new ByteArrayContent(ms.ToArray());
                        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = path
                        };

                        result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                        return result;
                    }
                }
            }
        }

        [HttpPut]
        public async Task PutAsync(ApiCamera apiCamera)
        {
            var camera = (await this.cameraRepository.ReadAsync(x => x.Number == apiCamera.Number).ConfigureAwait(false)).Single();
            var places = apiCamera.Places.Select(
                                      x => new Place
                                      {
                                          X = x.X,
                                          Y = x.Y,
                                          Height = x.Height,
                                          Width = x.Width,
                                          Id = x.Id
                                      })
                                  .ToList();

            camera.Places = places;
            await this.cameraRepository.WriteAsync(camera).ConfigureAwait(false);
            lock (Cache.Locker)
            {
                Cache.Cameras[camera.Number] = camera;
            }
        }
    }
}