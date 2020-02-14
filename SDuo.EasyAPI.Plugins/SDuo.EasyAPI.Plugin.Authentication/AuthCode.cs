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

            string authcode = context.Request.Headers[nameof(authcode)];

            if (string.IsNullOrEmpty(authcode))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(authcode));
                return 3;
            }
            
            XmlNodeList code = action.SelectNodes(nameof(code));

            foreach (XmlNode node in code)
            {
                if (string.IsNullOrEmpty(node.InnerText)) { continue; }
                if (string.Equals(authcode, node.InnerText, StringComparison.OrdinalIgnoreCase))
                {
                    callback?.Invoke(authcode);
                    return 0;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(authcode));
            return 4;
        }
    }
}