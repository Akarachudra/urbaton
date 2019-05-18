using Mongo.Service.Core.Statistics;
using Owin;

namespace Mongo.Service.Core.WebApp
{
    using MiddlwareFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    static class RequestCountersMiddlware
    {
        public static IAppBuilder UseRequestCounters(this IAppBuilder app, IRequestCounters requestCounters)
        {
            app.Use(UseRequestCountersFunc(requestCounters));
            return app;
        }

        public static MiddlwareFunc UseRequestCountersFunc(IRequestCounters requestCounters)
        {
            return
                next =>
                    async env =>
                    {
                        try
                        {
                            requestCounters.OnBeginRequest();

                            await next(env);
                        }
                        finally
                        {
                            requestCounters.OnEndRequest();
                        }
                    };
        }
    }
}