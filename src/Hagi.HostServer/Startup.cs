using Hagi.Shared.Network;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hagi.HostServer
{
    using Auth;
    using Configuration;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<AppSettings>(this.Configuration.GetSection(AppSettings.SectionName));
            services.AddSingleton<AppSettings>(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value.Initialise());
            services.AddSingleton<Config>();
            services.AddSingleton<Paths>();

            services.AddSingleton<HostDetection>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (false && env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseRouting();

            app.UseGuestAuth();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.ApplicationServices.GetRequiredService<Config>();

            HostDetection hostDetection = app.ApplicationServices.GetRequiredService<HostDetection>();
            lifetime.ApplicationStarted.Register(() => hostDetection.Listen());
            lifetime.ApplicationStopping.Register(() => hostDetection.Stop());

        }

    }
}