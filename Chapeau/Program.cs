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

            // Add services to the container
            builder.Services.AddControllersWithViews();
            builder.Services.AddLogging();

            // Register Repositories
            builder.Services.AddScoped<Repositories.MenuRepository>();
            builder.Services.AddScoped<Repositories.EmployeeRepository>();
            builder.Services.AddScoped<Repositories.CategoryRepository>();
            builder.Services.AddScoped<Repositories.RoleRepository>();
            builder.Services.AddScoped<Repositories.StatusRepository>();

            // Register Services
            builder.Services.AddScoped<Services.MenuService>();
            builder.Services.AddScoped<Services.EmployeeService>();
            builder.Services.AddScoped<Services.CategoryService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

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
