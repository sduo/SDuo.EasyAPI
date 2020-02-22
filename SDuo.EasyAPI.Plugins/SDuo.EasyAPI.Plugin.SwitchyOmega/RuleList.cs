using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.SwitchyOmega
{
    public class RuleList : IPlugin
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

            if (data == null)
            {
                context.Response.StatusCode=(int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(data));
                return 2;
            }

            using (DataTable table = data as DataTable)
            {
                if(table == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(data));
                    return 3;
                }                

                if (table.Rows.Count == 0)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return 0;
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "text/plain";

                await context.Response.WriteAsync("[SwitchyOmega Conditions]\n");

                string require = action.Attributes[nameof(require)]?.Value;

                if (!string.IsNullOrEmpty(require))
                {
                    await context.Response.WriteAsync($"; Require: SwitchyOmega >= {require}\n\n");
                }                

               foreach (DataRow row in  table.Rows)
                {
                    
                    string rule = row[nameof(rule)] as string;
                    if (string.IsNullOrEmpty(rule))
                    {
                        continue;
                    }
                    long? type = row[nameof(type)] as long?;
                    switch (type)
                    {
                        case 200:
                        case 210:
                            {
                                await context.Response.WriteAsync($"*.{rule}\n");
                                break;
                            }
                        case 201:
                        case 211:
                            {
                                await context.Response.WriteAsync($"{rule}\n");
                                break;
                            }                        
                    }                    
                }

                await context.Response.WriteAsync($"\n; Update: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
            return 0;
        }
    }
}
