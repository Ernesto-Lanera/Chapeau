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

            builder.Services.AddControllersWithViews();
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
            // Register Repositories - Map interfaces to implementations
            builder.Services.AddScoped<Repositories.Menu.IMenuRepository, Repositories.Menu.MenuRepository>();
            builder.Services.AddScoped<Repositories.Category.ICategoryRepository, Repositories.Category.CategoryRepository>();
            builder.Services.AddScoped<Repositories.EmployeeRepository>();
            builder.Services.AddScoped<Repositories.RoleRepository>();

            // Register Services
            builder.Services.AddScoped<Services.AuthService>();
            builder.Services.AddScoped<Services.IAuthService>(provider => provider.GetRequiredService<Services.AuthService>());
            builder.Services.AddScoped<Services.IClaimsService, Services.ClaimsService>();
            builder.Services.AddScoped<Services.MenuService>();
            builder.Services.AddScoped<Services.EmployeeService>();
            builder.Services.AddScoped<Services.CategoryService>();

            var app = builder.Build();

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
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}