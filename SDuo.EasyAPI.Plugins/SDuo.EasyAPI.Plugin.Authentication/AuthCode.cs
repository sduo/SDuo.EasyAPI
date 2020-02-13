using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.Authentication
{
    public class AuthCode : IPlugin
    {
        public string Version => "0.0.1";

        public async Task<int> ExecuteAsync(HttpContext context, XmlNode action, object data, Action<object> callback)
        {
            if (context == null)
            {
                throw new NullReferenceException(nameof(context));
            }

            if (action == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(action));
                return 1;
            }
            
            if (!action.HasChildNodes)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(action));
                return 2;
            }

            string code = context.Request.Headers[nameof(code)];

            if (string.IsNullOrEmpty(code))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(code));
                return 3;
            }

            foreach (XmlNode node in action.ChildNodes)
            {
                if (string.IsNullOrEmpty(node.InnerText)) { continue; }
                if (code.Equals(node.InnerText, StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(code));
            return 4;
        }
    }
}
