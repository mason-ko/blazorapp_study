using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using autochat_blazorapp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using autochat_blazorapp.Hubs;
using Microsoft.EntityFrameworkCore;
using autochat_blazorapp.Models;
using autochat_blazorapp.Common;

namespace autochat_blazorapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            services.AddSingleton<UserListService>();

            services.AddControllers();

            services.AddSignalR();


            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                Console.WriteLine("�⺻ ����: " + options.IdleTimeout);
                // Set a short timeout for easy testing.
            //    options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            // Google Login

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie();
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = Configuration["Google:ClientId"];
                options.ClientSecret = Configuration["Google:ClientSecret"];
                options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                options.ClaimActions.MapJsonKey("urn:google:image", "picture");
            });
            // From: https://github.com/aspnet/Blazor/issues/1554
            // Adds HttpContextAccessor
            // Used to determine if a user is logged in
            // and what their username is
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();
            // Required for HttpClient support in the Blazor Client project
            services.AddHttpClient();
            services.AddScoped<HttpClient>();
            // Pass settings to other components
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<AccountContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("AccountContext")));
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.UseHttpContextItemsMiddleware();

            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapControllers();
            });
        }
    }
}
