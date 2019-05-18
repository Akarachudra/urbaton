using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Mongo.Service.Core.WebApp
{
    public class CaptureActionForOwinFilter : IActionFilter
    {
        public bool AllowMultiple => true;

        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var route = actionContext.RequestContext.RouteData.Route;
            var capturedAction = new CapturedAction
            {
                RouteTemplate = route.RouteTemplate
            };

            var owinContext = actionContext.Request.GetOwinContext();
            owinContext.SetCapturedAction(capturedAction);

            return continuation();
        }
    }
}