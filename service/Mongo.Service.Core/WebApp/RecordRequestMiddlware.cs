using System.Diagnostics;
using Microsoft.Owin;
using Mongo.Service.Core.Statistics;
using Owin;

namespace Mongo.Service.Core.WebApp
{
    using MiddlwareFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    static class RecordRequestMiddlware
    {
        public static IAppBuilder RecordRequest(this IAppBuilder app, IStatisticsRecorder recorder)
        {
            app.Use(RecordRequestFunc(recorder));
            return app;
        }

        public static MiddlwareFunc RecordRequestFunc(IStatisticsRecorder recorder)
        {
            return
                next =>
                    async env =>
                    {
                        var context = new OwinContext(env);
                        var request = context.Request;
                        var stopwatch = Stopwatch.StartNew();

                        try
                        {
                            await next(env);
                        }
                        finally
                        {
                            var capturedAction = context.GetCapturedAction();
                            var route = capturedAction?.RouteTemplate ??
                                        (request.Path.HasValue ? request.Path.Value : null);
                            var time = stopwatch.Elapsed;
                            recorder.RecordAction("request", request.Method, context.Response.StatusCode, time, route);
                        }
                    };
        }
    }
}