using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core.Controllers
{
    public class FeedbackController : ApiController
    {
        private readonly IEntityStorage<Feedback> feedbackRepository;

        public FeedbackController(IEntityStorage<Feedback> feedbackRepository)
        {
            this.feedbackRepository = feedbackRepository;
        }

        [HttpPost]
        public async Task CreateFeedbackAsync(ApiFeedback apiFeedback)
        {
            var feedback = new Feedback
            {
                CameraNumber = apiFeedback.CameraNumber,
                Text = apiFeedback.Text,
                Title = apiFeedback.Title,
                X = apiFeedback.X,
                Y = apiFeedback.Y
            };
            await this.feedbackRepository.WriteAsync(feedback).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IEnumerable<ApiFeedback>> GetAllFeedbacksAsync()
        {
            var feedbacks = await this.feedbackRepository.ReadAllAsync().ConfigureAwait(false);
            return feedbacks.Select(
                x => new ApiFeedback
                {
                    X = x.X,
                    Y = x.Y,
                    CameraNumber = x.CameraNumber,
                    Text = x.Text,
                    Title = x.Title
                });
        }
    }
}