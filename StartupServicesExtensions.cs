using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

namespace web
{
    public static class StartupServicesExtensions
    {
        public static IServiceCollection AddCustomSession(this IServiceCollection services)
        {
            // The Tempdata provider cookie is not essential. Make it essential so Tempdata is functional when tracking is disabled.
            services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.IsEssential = true);

            // Session state cookies are not essential. Session state isn't functional when tracking is disabled.
            // The following code makes session cookies essential:  options.Cookie.IsEssential = true;
            services.AddSession(options =>
            {
                var environment = FetchHostingEnvironment(services);
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.Name =" Globals.SessionNameValue";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            return services;
        }

        public static IServiceCollection AddCustomRazorPagesAndMvc(this IServiceCollection services)
        {
            //
            //  this was in pre-razor pages so mvc etc.
            //
            services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = true;
            });
            //
            //  this works for razor pages
            //
            services.AddRouting(option =>
            {
                option.AppendTrailingSlash = true;
                option.LowercaseUrls = true;
            });

            services
                .AddMvc()
                .AddRazorPagesOptions(options =>
                    {
                        // map requests for /robots.txt to the razor page /Robotstxt
                        options.Conventions.AddPageRoute("/robotstxt", "/robots.txt");
                        // lock down /Production to producers only via a filter
                        //options.Conventions.AddFolderApplicationModelConvention("/Production", model => model.Filters.Add(new OnlyProducersPageFilter(onlyProducersLogger, roleService, cookieManager)));
                    });

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddHttpContextAccessor();

            //
            //  handle amazon elastic load balancers or nginx. any sort of proxy like that.
            //
            services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

            return services;
        }

        private static IWebHostEnvironment FetchHostingEnvironment(IServiceCollection services)
        {
            var current = services.BuildServiceProvider();
            return current.GetService<IWebHostEnvironment>();
        }
    }
}
