namespace web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; set; }

        public IWebHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMemoryCache();
            //services.AddDistributedMemoryCache();
            //services.AddCustomSession();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = _ => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Globals.CookieNameValue";
                options.Cookie.Path = "/";
                options.Cookie.Domain = HostingEnvironment.IsDevelopment() ? "Globals.DevelopmentSubdomainValue" : "Globals.ProductionSubdomainValue";
            });

            services.AddHttpClient();

            services.AddCustomRazorPagesAndMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment hostingEnvironment)
        {
            app.UseForwardedHeaders();

            if (hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx => ctx.Context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "public,max-age=" + DefaultCacheDurationInSeconds()
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next().ConfigureAwait(false);
            });

            app.Use((context, next) => context.Request.Path.StartsWithSegments("/healthcheck")
                ? context.Response.WriteAsync("200: OK")
                : next()
            );

            app.UseRouting();
            //app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }

        public int DefaultCacheDurationInSeconds()
        {
            //const int week = 7*24*60*60*1;
            const int day = 24 * 60 * 60 * 1;
            //const int hour = 60*60*1;
            const int minute = 60 * 1;
            //const int second = 1;

            return HostingEnvironment.IsDevelopment() ? minute : day;
        }
    }
}
