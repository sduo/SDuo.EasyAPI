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
            string value = null;
            switch (context.Request.Method)
            {                
                case string method when method == HttpMethods.Get:
                    {
                        foreach (string key in context.Request.Query.Keys)
                        {
                            if (param.ContainsKey(key))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, $"{nameof(param)}:{key}");
                                return 5;
                            }
                            value = WebUtility.UrlDecode(context.Request.Form[key]);
                            param.Add(key, value);
                        }
                        break;
                    }
                case string method when method == HttpMethods.Post:
                default:
                    {
                        foreach (string key in context.Request.Form.Keys)
                        {
                            if (param.ContainsKey(key))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, $"{nameof(param)}:{key}");
                                return 5;
                            }
                            value = WebUtility.UrlDecode(context.Request.Form[key]);
                            param.Add(key, value);
                        }
                        break;
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