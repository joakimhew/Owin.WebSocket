using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;
using System;

namespace Owin.WebSocket
{
    public class WebSocketConnectionMiddleware<T> : OwinMiddleware where T : WebSocketConnection
    {
        private readonly Regex mMatchPattern;
        private readonly WebSocketConnection mWebSocketConnection;

        public WebSocketConnectionMiddleware(OwinMiddleware next, WebSocketConnection webSocketConnection)
            :base(next)
        {
            mWebSocketConnection = webSocketConnection;
        }

          public WebSocketConnectionMiddleware(OwinMiddleware next, WebSocketConnection webSocketConnection, Regex matchPattern)
            :this(next, webSocketConnection)
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

            return mWebSocketConnection.AcceptSocketAsync(context, matches);
        }
    }
}