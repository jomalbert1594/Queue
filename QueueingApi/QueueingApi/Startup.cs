using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QueueDataAccess.Models;
using QueueingApi.Helpers;
using QueueingApi.Middlewares;
using QueueingApi.Model;
using QueueingApi.RepoAndServices.Counters;
using QueueingApi.RepoAndServices.Transactions;
using QueueingApi.RepoAndServices.Devices;
using QueueingApi.RepoAndServices.CounterTypes;

namespace QueueingApi
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
            // Allows all server to access this api
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "bearer";
                options.DefaultChallengeScheme = "bearer";
            }).AddJwtBearer("bearer", options =>
            {
                // Configure JWT Bearer Auth to expect our security key
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    RequireExpirationTime = true,
                    //ValidIssuer = "http://localhost:62710/",
                    //ValidIssuer = "http://192.168.2.6/queue/",
                    ValidIssuer = "http://192.168.1.110/queue/",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("superSecretKey@345")),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }

                        return Task.CompletedTask;
                    },

                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for the hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/queueHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();

            services.AddDbContext<QueueDbContext>(
                ServiceLifetime.Transient); // set the dbcontext lifetime to transient

            // Services necessary for Queue App 
            services.AddTransient<CounterApiRepo>();
            services.AddTransient<CounterApiService>();
            services.AddTransient<TransactionApiRepo>();
            services.AddTransient<TransactionApiService>();
            services.AddTransient<DeviceApiService>();
            services.AddTransient<DeviceApiRepo>();
            services.AddTransient<CounterTypeRepo>();
            services.AddTransient<CounterTypeService>();

            services.AddSingleton<CounterLocator>(); // The collection of counters

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();
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
                // The default HSTS value is 30 days. You may want to change 
                // this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();

            // Any Exception will be blocked and sent to the device that have requested a service
            app.UseExceptionBlockerMiddleware(); 

            app.UseHttpsRedirection();

            app.UseSignalR(routes =>
            {
                routes.MapHub<QueueHub>("/queueHub", options =>
                {
    
                });
            });

            app.UseCors("CorsPolicy");
            app.UseMvc();
        }
    }
}
