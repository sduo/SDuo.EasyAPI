using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Plugin.Kitsunebi
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
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.Headers.Add(IPlugin.X_PLUGIN_MESSAGE, nameof(data));
                return 2;
            }

            using (DataTable table = data as DataTable)
            {
                if (table == null)
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

                await context.Response.WriteAsync("[Rule]\n\n");

                foreach (DataRow row in table.Rows)
                {

                    string rule = row[nameof(rule)] as string;
                    if (string.IsNullOrEmpty(rule))
                    {
                        continue;
                    }
                    long? type = row[nameof(type)] as long?;
                    switch (type)
                    {
                        case 2:
                            {
                                await context.Response.WriteAsync($"IP-CIDR,{rule},PROXY\n");
                                break;
                            }
                        case 1:
                            {
                                await context.Response.WriteAsync($"DOMAIN-FULL,{rule},PROXY\n");
                                break;
                            }
                        case 0:
                        default:
                            {
                                await context.Response.WriteAsync($"DOMAIN-SUFFIX,{rule},PROXY\n");
                                break;
                            }
                    }
                }

                await context.Response.WriteAsync($"\nFINAL,DIRECT\n");

                XmlNodeList ds = action.SelectNodes(nameof(ds));

                if (ds.Count>0)
                {
                    await context.Response.WriteAsync($"\n[DnsServer]\n");

                    foreach(XmlNode n in ds)
                    {
                        await context.Response.WriteAsync($"{n.InnerText}\n");
                    }                    

                    foreach (DataRow row in table.Rows)
                    {

                        string rule = row[nameof(rule)] as string;
                        if (string.IsNullOrEmpty(rule))
                        {
                            continue;
                        }
                        long? dns = row[nameof(dns)] as long?;
                        if (dns > 0)
                        {
                            long? type = row[nameof(type)] as long?;
                            switch (type)
                            {
                                case 2:
                                    {
                                        continue;
                                    }
                                case 1:
                                    {
                                        await context.Response.WriteAsync($"DOMAIN-FULL,{rule},PROXY\n");
                                        break;
                                    }
                                case 0:
                                default:
                                    {
                                        await context.Response.WriteAsync($"DOMAIN-SUFFIX,{rule},PROXY\n");
                                        break;
                                    }
                            }
                        }
                    }
                }

                XmlNodeList host = action.SelectNodes(nameof(host));

                if (host.Count > 0)
                {
                    await context.Response.WriteAsync($"\n[DnsHost]\n");

                    foreach(XmlNode h in host)
                    {
                        await context.Response.WriteAsync($"{h.InnerText}\n");
                    }                    
                }

                string rds = action.Attributes[nameof(rds)]?.Value ?? "AsIs";
                await context.Response.WriteAsync($"\n[RoutingDomainStrategy]\n");
                await context.Response.WriteAsync($"\n{rds}\n");
            }
            return 0;
        }
    }
}