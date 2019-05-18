using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Kontur.Logging;

namespace Mongo.Service.Core.WebApp
{
    public class ExceptionLogger : IExceptionLogger
    {
        private readonly ILog log;

        public ExceptionLogger(ILog log)
        {
            this.log = log;
        }

        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            log.Error($"Exception occurred: {context.Exception.Message}{Environment.NewLine}{context.Exception.StackTrace}");
            return Task.FromResult(0);
        }
    }
}