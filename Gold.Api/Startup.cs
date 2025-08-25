using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ben.Diagnostics;
using DocumentFormat.OpenXml.EMMA;
using Gold.Api.Robots;
using Gold.Api.Services;
using Gold.Api.Services.Interfaces;
using Gold.Api.Utilities;
using Gold.Core.Vendors;
using Gold.Domain;
using Gold.Domain.Settings;
using Gold.Domain.Users.Interfaces;
using Gold.Domain.Users.Repositories;
using Gold.Domain.Vendors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;


namespace WebApplication2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            Authenticator.Key = "d4cead089e5b5a2931f1c83b1a45ac2548d012e2d4cead089e5b5a2931f1c83b1a45ac2548d012e2";
           
        }
 
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Debug("Started services");

            services.AddHttpClient();
            //services.AddDbContextPool<VendorsDbContext>(x => x.UseSqlServer(ConnectionSettings.Cstring2), 9999);
            services.AddDbContext<VendorsDbContext>(x => x.UseSqlServer(ConnectionSettings.Cstring2), ServiceLifetime.Transient);
            services.AddTransient<IVendorsRepository, VendorsRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddSingleton<IVendorBuySellService, VendorBuySellFinalizeService>();
            services.AddSingleton<IExpectedChecker, VendorExpectedChecker>();
            services.AddSingleton<ICashSender, CashSenderBot>();
            services.AddSingleton<CashSenderService>();
            services.AddSingleton<ExpectedCashService>();
            
            services.AddHostedService<KTApiService>();
            services.AddHostedService<KuwaitAccessHandler>();
            services.AddHostedService<BgServiceStarter<CashSenderService>>();
            services.AddHostedService<BgServiceStarter<ExpectedCashService>>();

            AIService.StartUp();
            VendorTransactionNew.InitiateTheDayOrFirstStart();

            KampanyaService.ResetPassword();
            AdminHelperService.StartAtStartUp();


            services.AddControllers();
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .AddXmlDataContractSerializerFormatters()
                .AddXmlSerializerFormatters();

            services.AddResponseCompression();
            
            services.AddControllers().AddXmlSerializerFormatters();
            services.AddControllers().AddXmlDataContractSerializerFormatters();
            //  services.AddControllers().AddXmlOptions();
            
            services.AddSwaggerGen(c =>
            {
                var info = new OpenApiInfo { Title = "GoldTag Server API", Version = "v1" };
                c.SwaggerDoc("v1", info) ;
            });

            var key = Encoding.ASCII.GetBytes(Authenticator.Key);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

           

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "API V1");
                    // c.RoutePrefix = string.Empty;
                });

            }
            /*
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "API V1");
                // c.RoutePrefix = string.Empty;
            });
            */
           
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();
            app.UseResponseCompression();
            //app.UseBlockingDetection();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
