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

            if (action == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(action));
                return 1;
            }

            string datasource = action.Attributes[nameof(datasource)].Value;
            if (string.IsNullOrEmpty(datasource))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(datasource));
                return 2;
            }

            string sql = action.SelectSingleNode(nameof(sql))?.InnerText;
            if (string.IsNullOrEmpty(sql))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(sql));
                return 3;
            }

            Dictionary<string, string> param = new Dictionary<string, string>();
            string value = null;
            foreach (string key in context.Request.Form.Keys)
            {
                if (param.ContainsKey(key))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, $"{nameof(param)}:{key}");
                    return 4;
                }
                value = WebUtility.UrlDecode(context.Request.Form[key]);
                if (!string.IsNullOrEmpty(value))
                {
                    param.Add(key, value);
                }
            }

            using (SQLiteConnection connection = new SQLiteConnection($"data source={datasource}"))
            {
                data = await connection.ExecuteAsync(sql, param).ConfigureAwait(false);
                callback?.Invoke(data);
                return 0;
            }
        }
    }
}