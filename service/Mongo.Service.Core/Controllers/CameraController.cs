using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<ApiCameraInfo>> GetAllAsync()
        {
            var cameras = await this.cameraRepository.ReadAllAsync().ConfigureAwait(false);
            return cameras.Select(
                x => new ApiCameraInfo
                {
                    Description = x.Description,
                    Number = x.Number
                });
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
            Cache.Cameras[camera.Number] = camera;
        }
    }
}