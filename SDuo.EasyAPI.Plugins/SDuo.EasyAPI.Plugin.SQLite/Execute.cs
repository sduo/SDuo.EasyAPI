using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.SQLite
{
    public class Execute : IPlugin
    {
        public string Version => "0.0.1";

        public async Task<int> ExecuteAsync(HttpContext context, XmlNode action, object data, Action<object> callback)
        {
            if (context == null)
            {
                throw new NullReferenceException(nameof(context));
            }

            if(context.Request.Method != HttpMethods.Post)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return -1;
            }

            if (action == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(action));
                return 1;
            }

            XmlNode sql = action.SelectSingleNode(nameof(sql));

            if (sql == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(sql));
                return 2;
            }

            string datasource = sql.Attributes[nameof(datasource)]?.Value;
            if (string.IsNullOrEmpty(datasource))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(datasource));
                return 3;
            }

            string cmd = sql.InnerText;
            if (string.IsNullOrEmpty(cmd))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(cmd));
                return 4;
            }

            Dictionary<string, string> param = new Dictionary<string, string>();
            string value = null;
            foreach (string key in context.Request.Form.Keys)
            {
                if (param.ContainsKey(key))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, $"{nameof(param)}:{key}");
                    return 5;
                }
                value = WebUtility.UrlDecode(context.Request.Form[key]);
                if (!string.IsNullOrEmpty(value))
                {
                    param.Add(key, value);
                }
            }

            using (SQLiteConnection connection = new SQLiteConnection($"data source={datasource}"))
            {
                data = await connection.ExecuteAsync(cmd, param).ConfigureAwait(false);
                callback?.Invoke(data);
                return 0;
            }
        }
    }
}