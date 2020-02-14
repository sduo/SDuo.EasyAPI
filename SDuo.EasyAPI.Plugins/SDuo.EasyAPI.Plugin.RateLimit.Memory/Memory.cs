using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.RateLimit
{
    public class Memory : IPlugin
    {
        public string Version => "0.0.1";

        public Task<int> ExecuteAsync(HttpContext context, XmlNode action, object data, Action<object> callback)
        {
            throw new NotImplementedException();
        }
    }
}
