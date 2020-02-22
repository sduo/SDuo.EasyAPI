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
        private const string DOMAIN_FULL = "DOMAIN-FULL";
        private const string IP_CIDR ="IP-CIDR";
        private const string DOMAIN_SUFFIX = "DOMAIN-SUFFIX";
        private const string USER_AGENT="USER-AGENT";

        private const string PROXY = "PROXY";
        private const string DRIECT = "DRIECT";
        private const string REJECT= "REJECT";

        private const string FINAL = "FINAL";
        private const string ASIS = "AsIs";

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
                        case 100:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_SUFFIX},{rule},{DRIECT}\n");
                                break;
                            }
                        case 101:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_FULL},{rule},{DRIECT}\n");
                                break;
                            }
                        case 102:
                            {
                                await context.Response.WriteAsync($"{IP_CIDR},{rule},{DRIECT}\n");
                                break;
                            }
                        case 103:
                            {
                                await context.Response.WriteAsync($"{USER_AGENT},{rule},{DRIECT}\n");
                                break;
                            }
                        case 200:
                        case 220:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_SUFFIX},{rule},{PROXY}\n");
                                break;
                            }
                        case 201:
                        case 221:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_FULL},{rule},{PROXY}\n");
                                break;
                            }
                        case 202:
                            {
                                await context.Response.WriteAsync($"{IP_CIDR},{rule},{PROXY}\n");
                                break;
                            }
                        case 203:
                            {
                                await context.Response.WriteAsync($"{USER_AGENT},{rule},{PROXY}\n");
                                break;
                            }                        
                        case 900:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_SUFFIX},{rule},{REJECT}\n");
                                break;
                            }
                        case 901:
                            {
                                await context.Response.WriteAsync($"{DOMAIN_FULL},{rule},{REJECT}\n");
                                break;
                            }
                        case 902:
                            {
                                await context.Response.WriteAsync($"{IP_CIDR},{rule},{REJECT}\n");
                                break;
                            }
                        case 903:
                            {
                                await context.Response.WriteAsync($"{USER_AGENT},{rule},{REJECT}\n");
                                break;
                            }
                    }
                }

                string final = action.Attributes[nameof(final)]?.Value;

                if (string.IsNullOrEmpty(final))
                {
                    final = DRIECT;
                }

                await context.Response.WriteAsync($"\n{FINAL},{final}\n");

                XmlNode dns = action.SelectSingleNode(nameof(dns));

                if (dns != null)
                {
                    XmlNode servers = dns.SelectSingleNode(nameof(servers));
                    if(servers?.HasChildNodes == true)
                    {
                        XmlNodeList server = servers.SelectNodes(nameof(server));
                        if (server.Count > 0)
                        {
                            await context.Response.WriteAsync($"\n[DnsServer]\n");
                            foreach (XmlNode node in server)
                            {
                                await context.Response.WriteAsync($"{node.InnerText}\n");
                            }
                        }
                    }

                    await context.Response.WriteAsync($"\n[DnsRule]\n");

                    foreach (DataRow row in table.Rows)
                    {

                        string rule = row[nameof(rule)] as string;
                        if (string.IsNullOrEmpty(rule))
                        {
                            continue;
                        }
                        if ((row["dns"] as long?) > 0)
                        {
                            switch (row["type"] as long?)
                            {
                                case 100:
                                    {
                                        await context.Response.WriteAsync($"{DOMAIN_SUFFIX},{rule},{DRIECT}\n");
                                        break;
                                    }
                                case 101:
                                    {
                                        await context.Response.WriteAsync($"{DOMAIN_FULL},{rule},{DRIECT}\n");
                                        break;
                                    }
                                case 200:
                                    {
                                        await context.Response.WriteAsync($"{DOMAIN_SUFFIX},{rule},{PROXY}\n");
                                        break;
                                    }
                                case 201:
                                    {
                                        await context.Response.WriteAsync($"{DOMAIN_FULL},{rule},{PROXY}\n");
                                        break;
                                    }
                            }
                        }
                    }

                    XmlNode hosts = dns.SelectSingleNode(nameof(hosts));
                    if(hosts?.HasChildNodes == true)
                    {
                        XmlNodeList host = hosts.SelectNodes(nameof(host));
                        if (host.Count > 0)
                        {
                            await context.Response.WriteAsync($"\n[DnsHost]\n");

                            foreach (XmlNode h in host)
                            {
                                await context.Response.WriteAsync($"{h.InnerText}\n");
                            }
                        }
                    }
                }


                string rds = action.Attributes[nameof(rds)]?.Value;
                if (string.IsNullOrEmpty(rds))
                {
                    rds = ASIS;
                }
                await context.Response.WriteAsync($"\n[RoutingDomainStrategy]\n");
                await context.Response.WriteAsync($"{rds}");
            }
            return 0;
        }
    }
}