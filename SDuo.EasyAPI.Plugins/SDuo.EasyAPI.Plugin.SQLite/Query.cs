using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;
using Dapper;
using Microsoft.Extensions.Primitives;
using System.Data;
using System.Data.Common;

namespace SDuo.EasyAPI.Plugin.SQLite
{
    public class Query : IPlugin
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

            if (context.Request.Method == HttpMethods.Post)
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
            }            

            using (SQLiteConnection connection = new SQLiteConnection(datasource))
            using (DbDataReader reader = await connection.ExecuteReaderAsync(cmd, param).ConfigureAwait(true))
            using(DataTable table = new DataTable())
            {
                table.Load(reader);
                callback?.Invoke(table);
                return 0;
            }
        }
    }
}
