using Chapeau.Repositories;
using Chapeau.Constants.Login;
using Chapeau.Middleware;
using Chapeau.Repositories.Financial;
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
            }).AddSessionStateTempDataProvider();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

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
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.PrepareFood));

                options.AddPolicy("CanPrepareDrinks", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.PrepareDrinks));

                options.AddPolicy("CanManageEmployees", policy =>
                    policy.RequireClaim("Permission", "ManageEmployees"));

                options.AddPolicy("CanManageMenuItems", policy =>
                    policy.RequireClaim("Permission", "ManageMenuItems"));

                options.AddPolicy("CanManageStock", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.ManageStock));

                options.AddPolicy("CanViewFinance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.ViewFinance) ||
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.LegacyViewReports)));

                // Backwards compatible policy for older non-management controller examples.
                options.AddPolicy("CanViewReports", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.ViewFinance) ||
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.LegacyViewReports)));

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

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Register repositories and services
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Scenario 5: repositories via interfaces
            builder.Services.AddScoped<Repositories.Menu.IMenuRepository, Repositories.Menu.MenuRepository>();
            builder.Services.AddScoped<Repositories.IEmployeeRepository, Repositories.EmployeeRepository>();
            builder.Services.AddScoped<Repositories.Category.ICategoryRepository, Repositories.Category.CategoryRepository>();
            builder.Services.AddScoped<Repositories.IRoleRepository, Repositories.RoleRepository>();

            // Scenario 5: application services via interfaces
            builder.Services.AddScoped<Services.IMenuService, Services.MenuService>();
            builder.Services.AddScoped<Services.IStockService, Services.StockService>();
            builder.Services.AddScoped<Services.ICategoryService, Services.CategoryService>();
            builder.Services.AddScoped<Services.IImageService, Services.ImageService>();
            builder.Services.AddScoped<Services.Overview.IEmployeeService, Services.Overview.EmployeeService>();

            // Concrete registrations remain for existing non-management controllers.
            builder.Services.AddScoped<Services.MenuService>();
            builder.Services.AddScoped<Services.CategoryService>();
            builder.Services.AddScoped<Services.ImageService>();
            builder.Services.AddScoped<Services.OrderService>();

            builder.Services.AddScoped<Services.Login.IAuthService, Services.Login.AuthService>();
            builder.Services.AddScoped<Services.Login.IClaimsService, Services.Login.ClaimsService>();
            builder.Services.AddScoped<Services.Login.IDashboardRouterService, Services.Login.DashboardRouterService>();

            // Register Financial services and repositories
            builder.Services.AddScoped<IFinancialRepository, FinancialRepository>();
            builder.Services.AddScoped<IFinancialService, FinancialService>();

            // Register Table repository
            builder.Services.AddScoped<ITableRepository, TableRepository>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var tableRepo = scope.ServiceProvider.GetRequiredService<ITableRepository>();
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

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            app.UseMiddleware<PermissionClaimsRefreshMiddleware>();


            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}