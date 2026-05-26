using Chapeau.Repositories;
using Chapeau.Services;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Chapeau
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var defaultCulture = new CultureInfo("nl-NL");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
            });

            builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
            {
                options.ViewLocationFormats.Add("Views/Login/{0}.cshtml");
                options.ViewLocationFormats.Add("Views/Overview/{0}.cshtml");
            });
            builder.Services.AddLogging();

            // Add Response Compression
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            // Add Authentication
            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            // Add Authorization with permission-based policies
            builder.Services.AddAuthorization(options =>
            {
                // Permission-based policies
                options.AddPolicy("CanViewMenu", policy =>
                    policy.RequireClaim("Permission", "ViewMenu"));

                options.AddPolicy("CanTakeOrders", policy =>
                    policy.RequireClaim("Permission", "TakeOrders"));

                options.AddPolicy("CanPrepareFood", policy =>
                    policy.RequireClaim("Permission", "PrepareFood"));

                options.AddPolicy("CanManageEmployees", policy =>
                    policy.RequireClaim("Permission", "ManageEmployees"));

                options.AddPolicy("CanManageMenuItems", policy =>
                    policy.RequireClaim("Permission", "ManageMenuItems"));

                options.AddPolicy("CanViewReports", policy =>
                    policy.RequireClaim("Permission", "ViewReports"));

                options.AddPolicy("CanManageRoles", policy =>
                    policy.RequireClaim("Permission", "ManageRoles"));

                // Role-based policies
                options.AddPolicy("IsManager", policy =>
                    policy.RequireRole("Manager"));

                options.AddPolicy("IsWaiter", policy =>
                    policy.RequireRole("Waiter"));

                options.AddPolicy("IsKitchenStaff", policy =>
                    policy.RequireRole("Kitchen"));
            });

            // Register Repositories
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddLogging();

            // Register Repositories - Map interfaces to implementations
            builder.Services.AddScoped<Repositories.Menu.IMenuRepository, Repositories.Menu.MenuRepository>();
            builder.Services.AddScoped<Repositories.EmployeeRepository>();
            builder.Services.AddScoped<Repositories.Category.ICategoryRepository, Repositories.Category.CategoryRepository>();
            builder.Services.AddScoped<Repositories.RoleRepository>();

            // Register Services
            builder.Services.AddScoped<Services.MenuService>();
            builder.Services.AddScoped<Services.CategoryService>();
            builder.Services.AddScoped<Services.ImageService>();

            builder.Services.AddScoped<Services.Login.IAuthService, Services.Login.AuthService>();
            builder.Services.AddScoped<Services.Login.IClaimsService, Services.Login.ClaimsService>();
            builder.Services.AddScoped<Services.Login.IDashboardRouterService, Services.Login.DashboardRouterService>();

            builder.Services.AddScoped<Services.Overview.EmployeeService>();
            builder.Services.AddScoped<TableRepository>();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var tableRepo = scope.ServiceProvider.GetRequiredService<TableRepository>();
                tableRepo.EnsureColumnExists();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}