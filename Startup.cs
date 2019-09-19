using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using netMIH;
using netPDQContainer.collections;
using netPDQContainer.hubs;
using netPDQContainer.services;

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
            services.AddSignalR();
            var i = new netMIH.Index();
            foreach (var f in Directory.EnumerateFiles("hashes","*.PDQ"))
            {
                var newHashes = File.ReadAllLines(f);
                for (int counter = 0; counter < newHashes.Length; counter++)
                {
                    newHashes[counter] = newHashes[counter].ToLower().Trim();
                }
                Console.WriteLine($"Loading {newHashes.Length} hashes from {f}");
                i.Update(newHashes, Path.GetFileName(f).Replace(".PDQ",""));                
            }

            i.Train();
            Console.WriteLine($"Index reports {i.Count()} unique entries");
            services.AddSingleton<netMIH.Index>(i);
            services.AddSingleton<PDQWrapper>(new PDQWrapper("/facebook/hashing/pdq/cpp/pdq-photo-hasher"));
            //services.AddSingleton<PDQWrapper>(new PDQWrapper("/home/janisd/PycharmProjects/ThreatExchange/hashing/pdq/cpp/pdq-photo-hasher"));
            
            services.AddSingleton<IBackgroundTaskQueue<Tuple<byte[], string, string>>, BackgroundQueue<Tuple<byte[], string, string>>>();
            services.AddHostedService<HashService>();
            services.AddSingleton<IBackgroundTaskQueue<Tuple<string, int, string>>, BackgroundQueue<Tuple<string, int, string>>>();
            services.AddHostedService<SearchService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
        }
    }
}