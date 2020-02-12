using System;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;

namespace SDuo.EasyAPI.Plugin.Abstract
{
    public interface IPlugin
    {
        public const string NameSpace = "SDUO.EasyAPI.Plugin";
        public const string X_PLUGIN_MESSAGE = "X-Plugin-Message";

        string Version { get; }
        Task<int> ExecuteAsync(HttpContext context, XmlNode action, object data, Action<object> callback);
    }
}