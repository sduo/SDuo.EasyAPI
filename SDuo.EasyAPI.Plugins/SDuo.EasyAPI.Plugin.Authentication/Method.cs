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
    public class Method : IPlugin
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

            string methods = action.Attributes[nameof(methods)]?.Value;

            if (string.IsNullOrEmpty(methods))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(methods));
                return 2;
            }

            string[] array = methods.Split(',');

            foreach(string method in array)
            {
                if(string.Equals(method, context.Request.Method, StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(methods));
            return 2;
        }
    }
}