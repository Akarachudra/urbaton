using Microsoft.Owin;

namespace Mongo.Service.Core.WebApp
{
    internal static class OwinCapturedActionExtensions
    {
        private const string ContextKey = "app.CapturedAction";

        public static void SetCapturedAction(this IOwinContext owinContext, CapturedAction capturedAction)
        {
            owinContext.Environment[ContextKey] = capturedAction;
        }

        public static CapturedAction GetCapturedAction(this IOwinContext owinContext)
        {
            object obj;
            if (owinContext.Environment.TryGetValue(ContextKey, out obj))
            {
                return obj as CapturedAction;
            }

            return null;
        }
    }
}