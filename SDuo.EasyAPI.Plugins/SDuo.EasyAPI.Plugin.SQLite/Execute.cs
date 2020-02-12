using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.SQLite
{
    public class Execute : IPlugin
    {
        public string Version => throw new NotImplementedException();

        public Task<int> ExecuteAsync(HttpContext context, XmlNode action, object data, Action<object> callback)
        {
            throw new NotImplementedException();
        }
    }
}