using Chapeau.Constants.Login;
using Chapeau.Middleware;
using Chapeau.Repositories;
using Chapeau.Repositories.Financial;
using Chapeau.Repositories.Payment;
using Chapeau.Services;
using Chapeau.Services.Payment;
using Microsoft.AspNetCore.Authentication;
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
                    .RequireClaim(ClaimTypeConstants.IsActive, bool.TrueString)
                    .Build();

                options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
            }).AddSessionStateTempDataProvider();

            builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
            {
                options.ViewLocationFormats.Add("Views/Login/{0}.cshtml");
                options.ViewLocationFormats.Add("Views/Overview/{0}.cshtml");
            });

            builder.Services.AddLogging();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddScoped<IClaimsTransformation, Services.Login.PermissionClaimsTransformation>();

            // Session wordt gebruikt voor TempData en kleine gebruikersinformatie.
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Response compression houdt de pagina's iets kleiner tijdens het laden.
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization(options =>
            {
                // Permission policies voor de verschillende delen van de applicatie.
                options.AddPolicy("CanTakeOrders", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.TakeOrders));

                options.AddPolicy("CanPrepareFood", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.PrepareFood));

                options.AddPolicy("CanPrepareDrinks", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.PrepareDrinks));

                options.AddPolicy("CanManageEmployees", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.ManageEmployees));

                options.AddPolicy("CanManageMenuItems", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.ManageMenuItems));

                options.AddPolicy("CanManageStock", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.ManageStock));

                options.AddPolicy("CanManageRoles", policy =>
                    policy.RequireClaim(ClaimTypeConstants.Permission, PermissionConstants.ManageRoles));

                options.AddPolicy("CanViewFinance", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.ViewFinance) ||
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.LegacyViewReports)));

                // Oude naam blijft werken voor bestaande controllers of oude links.
                options.AddPolicy("CanViewReports", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.ViewFinance) ||
                        context.User.HasClaim(ClaimTypeConstants.Permission, PermissionConstants.LegacyViewReports)));

                options.AddPolicy("IsManager", policy => policy.RequireRole("Manager"));
                options.AddPolicy("IsWaiter", policy => policy.RequireRole("Waiter"));
                options.AddPolicy("IsKitchenStaff", policy => policy.RequireRole("Kitchen"));
            });

            // Bestaande order en payment code.
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<ITableRepository, TableRepository>();

            // Scenario 5 repositories.
            builder.Services.AddScoped<IMenuRepository, MenuRepository>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IFinancialRepository, FinancialRepository>();

            // Scenario 5 services.
            builder.Services.AddScoped<IMenuService, MenuService>();
            builder.Services.AddScoped<IStockService, StockService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IFinancialService, FinancialService>();
            builder.Services.AddScoped<IRolePermissionsService, RolePermissionsService>();
            builder.Services.AddScoped<Services.Overview.IEmployeeService, Services.Overview.EmployeeService>();

            // Deze concrete services zijn er nog voor oudere controllers in het project.
            builder.Services.AddScoped<MenuService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ImageService>();
            builder.Services.AddScoped<OrderService>();

            // Login services.
            builder.Services.AddScoped<Services.Login.IAuthService, Services.Login.AuthService>();
            builder.Services.AddScoped<Services.Login.IClaimsService, Services.Login.ClaimsService>();
            builder.Services.AddScoped<Services.Login.IDashboardRouterService, Services.Login.DashboardRouterService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var tableRepository = scope.ServiceProvider.GetRequiredService<ITableRepository>();
                tableRepository.EnsureColumnExists();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();

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
