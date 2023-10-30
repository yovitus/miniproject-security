using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureAggregationAPI.Services;
using Microsoft.AspNetCore.Authorization;
using SecureAggregationAPI.Authorization;
using SecureAggregationAPI.Controllers;

namespace SecureAggregationAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<HospitalService>();
            services.AddSingleton<SecretSharingService>(); // Add SecretSharingService
            services.AddScoped<PatientsController>(); // Add PatientsController

            services.AddAuthorization(options =>
            {
                options.AddPolicy("HospitalApiKeyPolicy", policy =>
                {
                    policy.AddRequirements(new HospitalApiKeyAuthorizationRequirement());
                });
            });

            services.AddScoped<IAuthorizationHandler, HospitalApiKeyAuthorizationHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
