using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netPDQContainer.collections;
using netPDQContainer.hubs;
using netPDQContainer.services;
using Swashbuckle.AspNetCore.Swagger;
#pragma warning disable 1591

namespace netPDQContainer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PDQWrapper>(new PDQWrapper(Configuration["PDQ:Binary"]));
            services.AddSignalR();
            var i = new netMIH.Index();
            foreach (var f in Directory.EnumerateFiles("hashes","*.PDQ"))
            {
                var newHashes = File.ReadAllLines(f);
                for (var counter = 0; counter < newHashes.Length; counter++)
                {
                    newHashes[counter] = newHashes[counter].ToLower().Trim();
                }
                Console.WriteLine($"Loading {newHashes.Length} hashes from {f}");
                i.Update(newHashes, Path.GetFileName(f).Replace(".PDQ",""));                
            }

            i.Train();
            Console.WriteLine($"Index reports {i.Count()} unique entries");
            services.AddSingleton<netMIH.Index>(i);
            
            
            services.Configure<HashServiceOptions>(Configuration.GetSection("HashService"));
            services.AddSingleton<IBackgroundTaskQueue<Tuple<byte[], string, string>>, BackgroundQueue<Tuple<byte[], string, string>>>();
            services.AddHostedService<HashService>();
            services.Configure<SearchServiceOptions>(Configuration.GetSection("SearchService"));
            services.AddSingleton<IBackgroundTaskQueue<Tuple<string, int, string>>, BackgroundQueue<Tuple<string, int, string>>>();
            services.AddHostedService<SearchService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new Info { Title = "netPDQContainer", Version = "v1", 
                    License = new License()
                    {
                        Name = "MIT", 
                        Url = "https://github.com/AiLECS/netPDQContainer/blob/master/licence"
                    }, Contact = new Contact()
                    {
                        Email = "janis.dalins@monash.edu", 
                        Name = "Janis Dalins", 
                        Url = "https://github.com/AiLECS/netPDQContainer"
                    }, 
                    Description = "A .NET core based API exposing the PDQ perceptual hashing algorithm by @facebook (\"PDQ as a service\")"});
                c.IncludeXmlComments( Path.Combine(System.AppContext.BaseDirectory, "netPDQContainer.xml"));

                #region XMLDocumentationWorkaround
                // workaround for limitations regarding publication of XML comments from nuget dependencies within Swagger doc. 
                if (Directory.Exists(System.AppContext.BaseDirectory + "documentation"))
                {
                    foreach (var commentFile in Directory.EnumerateFiles(System.AppContext.BaseDirectory  
                                                                         + "documentation"))
                    {
                        c.IncludeXmlComments(commentFile);
                    }    
                }
                #endregion
                
                
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSignalR((routes) =>
            {
                routes.MapHub<SearchHub>("/PdqHub", options => {options.ApplicationMaxBufferSize = 200 * 1024; });
            });
            
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", ".net PDQ container");
                c.DocumentTitle = "netPDQContainer Swagger/OpenAPI spec";

            });
        }
    }
}
#pragma warning restore 1591