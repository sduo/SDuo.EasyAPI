using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDuo.EasyAPI.Plugin.Abstract;

namespace SDuo.EasyAPI.Core
{
    public class Startup
    {
        private const string X_ERROR_MESSAGE = "X-Error-Message";

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/{**action}", async context =>
                {
                    string action = context.Request.Path;

                    if (string.IsNullOrEmpty(action))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Headers.Add(X_ERROR_MESSAGE, nameof(action));
                        return;
                    }
                    
                    if('\\' == Path.DirectorySeparatorChar)
                    {
                        action = action.Replace('/', Path.DirectorySeparatorChar);
                    }

                    if (action.StartsWith(Path.DirectorySeparatorChar))
                    {
                        action = action.Remove(0, 1);
                    }

                    action = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "actions", $"{action}.xml");

                    if (!File.Exists(action))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Headers.Add(X_ERROR_MESSAGE, action);
                        return;
                    }

                    XmlDocument xml = new XmlDocument();
                    xml.Load(action);

                    XmlNode root = xml.SelectSingleNode("actions");

                    if (root?.HasChildNodes != true)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Headers.Add(X_ERROR_MESSAGE, action);
                        return;
                    }

                    object data = null;

                    foreach (XmlNode act in root.ChildNodes)
                    {
                        string plugin = act.Attributes[nameof(plugin)]?.Value;
                        if (string.IsNullOrEmpty(plugin))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, nameof(plugin));
                            return;
                        }

                        string invoke = act.Attributes[nameof(invoke)]?.Value;
                        if (string.IsNullOrEmpty(invoke))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, nameof(invoke));
                            return;
                        }

                        if (!plugin.StartsWith(IPlugin.NAMESPACE))
                        {
                            plugin = $"{IPlugin.NAMESPACE}.{plugin}";
                        }

                        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", plugin, $"{plugin}.dll");

                        if (!File.Exists(file))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, file);
                            return;
                        }

                        PluginLoadContext loader = new PluginLoadContext(file);
                        Assembly assembly = loader.LoadFromAssemblyName(new AssemblyName(plugin));

                        if (assembly == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, plugin);
                            return;
                        }
                        

                        invoke = $"{plugin}.{invoke}";
                        
                        IPlugin instance = assembly.CreateInstance(invoke) as IPlugin;                                            

                        if (instance == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, invoke);
                            return;
                        }

                        int result = await instance.ExecuteAsync(context, act, data, x => { data = x; }).ConfigureAwait(true);

                        loader?.Unload();

                        if (result != 0)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.Headers.Add(X_ERROR_MESSAGE, $"{invoke}:{result}");
                            return;
                        }
                    };
                });
            });
        }
    }
}
