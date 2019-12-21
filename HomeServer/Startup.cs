using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http.Features;

namespace HomeServer
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
            services.AddControllersWithViews();
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 8000000000;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "FileStorage")),
                RequestPath = "/FileStorage"
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "EditTable",
                    pattern: "DataWarehouse/Data/EditTable/{tableName}",
                    defaults: new { area = "DataWarehouse", controller = "Data", action = "EditTable" });
                endpoints.MapControllerRoute(
                    name: "CommandExecute",
                    pattern: "DataWarehouse/Data/CommandExecute/{name}",
                    defaults: new { area = "DataWarehouse", controller = "Data", action = "CommandExecute" });
                endpoints.MapControllerRoute(
                    name: "QueryExecute",
                    pattern: "DataWarehouse/Data/QueryExecute/{name}",
                    defaults: new { area = "DataWarehouse", controller = "Data", action = "QueryExecute" });
                endpoints.MapControllerRoute(
                    name: "Browse",
                    pattern: "FileManager/FileExplorer/Browse/{base64Path}",
                    defaults: new { area = "FileManager", controller = "FileExplorer", action = "Browse" });
                endpoints.MapControllerRoute(
                    name: "Browse",
                    pattern: "FileManager/FileExplorer/Download/{base64Path}",
                    defaults: new { area = "FileManager", controller = "FileExplorer", action = "Download" });
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller}/{action}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
