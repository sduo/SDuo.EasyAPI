using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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
        public string Version => "0.1.0";

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

            Dictionary<string, object> param = new Dictionary<string, object>();

            XmlNodeList args = action.SelectNodes(nameof(param));

            foreach (XmlNode arg in args)
            {
                string name = arg.Attributes[nameof(name)]?.Value;

                if (string.IsNullOrEmpty(name))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(param));
                    return 5;
                }

                if (param.ContainsKey(name))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, name);
                    return 6;
                }

                string from = arg.Attributes[nameof(from)]?.Value;

                if (string.Equals("body", from, StringComparison.OrdinalIgnoreCase))
                {
                    param.Add(name, WebUtility.UrlDecode(context.Request.Form[name]));
                }
                else if (string.Equals("query", from, StringComparison.OrdinalIgnoreCase))
                {
                    param.Add(name, WebUtility.UrlDecode(context.Request.Query[name]));
                }
            }

            using (SQLiteConnection connection = new SQLiteConnection(datasource))
            {
                data =await connection.ExecuteAsync(cmd, param).ConfigureAwait(true);
                callback?.Invoke(data);
                context.Response.StatusCode = (int)HttpStatusCode.Created;
                return 0;
            }
        }
    }
}