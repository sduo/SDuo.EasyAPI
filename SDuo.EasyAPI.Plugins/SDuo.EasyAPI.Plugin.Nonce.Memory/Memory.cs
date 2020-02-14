using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.Nonce
{
    public class Memory : IPlugin
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

            string authcode = data as string;

            if (string.IsNullOrEmpty(authcode))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(authcode));
                return 2;
            }

            string nonce = context.Request.Headers[nameof(nonce)];

            if (string.IsNullOrEmpty(nonce))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(authcode));
                return 3;
            }

            using (MemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions())))
            {
                string key = $"Nonce:{authcode}";

                string prefix = action.Attributes[nameof(prefix)]?.Value;
                if (!string.IsNullOrEmpty(prefix))
                {
                    key = string.Join(":", prefix, key);
                }
                string value = cache.Get(key) as string;
                if (string.Equals(nonce, value, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(authcode));
                    return 4;
                }
                else
                {
                    string  timeout  = action.Attributes[nameof(timeout)]?.Value;
                    if (!long.TryParse(timeout, out long ms))
                    {
                        ms = 5000 ;
                    }
                    cache.Set(key, value, TimeSpan.FromTicks(ms * TimeSpan.TicksPerMillisecond));
                }                
            }
            return 0;
        }
    }
}
