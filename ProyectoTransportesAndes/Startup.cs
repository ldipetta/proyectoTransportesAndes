using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Persistencia;

namespace ProyectoTransportesAndes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private IOptions<AppSettings> _settings;
        private IOptions<AppSettingsMongo> _settingsMongo;
        


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourDomain.com",
                    ValidAudience = "yourDomain.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gjanvowIINFDk4086206ldvnffnhsdonL"/*Configuration["Llave"]*/)),
                    ClockSkew = TimeSpan.Zero 
                });
            services.AddSession(s => s.IdleTimeout = TimeSpan.FromMinutes(30));
            services.AddMvc();
            services.AddOptions();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<AppSettingsMongo>(Configuration.GetSection("AppSettingsMongo"));
            services.AddDistributedMemoryCache();
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,IOptions<AppSettings> appSettings,IOptions<AppSettingsMongo>appSettingsMongo)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            _settings = appSettings;
            _settingsMongo = appSettingsMongo;
            
            //DBRepository<AppSettings>.Iniciar(_settings);
            DBRepositoryMongo<AppSettingsMongo>.Iniciar(_settingsMongo);

        }
    }
}
