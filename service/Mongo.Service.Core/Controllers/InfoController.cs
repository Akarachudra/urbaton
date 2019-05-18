using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    public class InfoController : ApiController
    {
        private readonly IEntityStorage<Info> infoRepository;

        public InfoController(IEntityStorage<Info> infoRepository)
        {
            this.infoRepository = infoRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<ApiInfo>> GetAllAsync()
        {
            var infos = await this.infoRepository.ReadAllAsync().ConfigureAwait(false);
            return infos.Select(
                x => new ApiInfo
                {
                    Description = x.Description,
                    CameraNumber = x.CameraNumber,
                    FreePlaces = x.FreePlaces,
                    OccupiedPlaces = x.OccupiedPlaces,
                    TotalPlaces = x.TotalPlaces
                });
        }
    }
}