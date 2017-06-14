using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using System;
using Ninject;

namespace Owin.WebSocket
{
    public class WebSocketConnectionMiddleware<T> : OwinMiddleware where T : WebSocketConnection
    {
        private readonly Regex mMatchPattern;
        private readonly IKernel mKernel;

        public WebSocketConnectionMiddleware(OwinMiddleware next, IKernel kernel)
            :base(next)
        {
            mKernel = kernel;
        }

          public WebSocketConnectionMiddleware(OwinMiddleware next, IKernel kernel, Regex matchPattern)
            :this(next, kernel)
        {
            mMatchPattern = matchPattern;
        }

        public override Task Invoke(IOwinContext context)
        {
            var matches = new Dictionary<string, string>();

            if (mMatchPattern != null)
            {
                var match = mMatchPattern.Match(context.Request.Path.Value);
                if(!match.Success)
                    return Next.Invoke(context);

                for (var i = 1; i <= match.Groups.Count; i++)
                {
                    var name = mMatchPattern.GroupNameFromNumber(i);
                    var value = match.Groups[i];
                    matches.Add(name, value.Value);
                }
            }

            T socketConnection;

            if(mKernel == null)
            {
                socketConnection = Activator.CreateInstance<T>();
            }
            else
            {
                var all = mKernel.GetAll<T>();

                socketConnection = mKernel.Get<T>();
            }

            return socketConnection.AcceptSocketAsync(context, matches);
        }
    }
}