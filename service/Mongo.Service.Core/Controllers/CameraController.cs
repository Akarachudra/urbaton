using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mongo.Service.Core.Entities;
using Mongo.Service.Core.Repository;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly IMongoRepository<Camera> cameraRepository;

        public CameraController(IMongoRepository<Camera> cameraRepository)
        {
            this.cameraRepository = cameraRepository;
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