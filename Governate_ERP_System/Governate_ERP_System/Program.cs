using Governate_ERP_System.Application.Interfaces;
using Governate_ERP_System.Infrastructure.Entities;
using Governate_ERP_System.Infrastructure.MultiTenancy;
using Governate_ERP_System.Infrastructure.Persistence;
using Governate_ERP_System.Infrastructure.Services;
using Governate_ERP_System.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Governate_ERP_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DbContext for Identity (main DB)
            builder.Services.AddDbContext<AppIdentityDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("Catalog")));

            builder.Services.AddScoped<ICurrentProjectAccessor, CurrentProjectAccessor>();
            builder.Services.AddScoped<IAccountingDbContextFactory, AccountingDbContextFactory>();
            builder.Services.AddScoped<IProjectProvisioner, ProjectProvisioner>();
            builder.Services.AddScoped<IJournalValidator, JournalValidator>();
            builder.Services.AddScoped<IAccountingService, AccountingService>();
            builder.Services.AddScoped<ITenantCatalogService, TenantCatalogService>();
            builder.Services.AddControllersWithViews();



           

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    // Password/Sign-in policies (tune as you need)
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = true;

                    // *** No email confirmation required ***
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedAccount = false;

                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            // Auth cookie paths
            builder.Services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/Login";
                opt.AccessDeniedPath = "/Account/AccessDenied";
                opt.LogoutPath = "/Account/Logout";
                opt.ExpireTimeSpan = TimeSpan.FromDays(7);
                opt.SlidingExpiration = true;
            });

            var app = builder.Build();
            app.UseMiddleware<ProjectResolutionMiddleware>();
            using (var scope = app.Services.CreateScope())
            {
                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                var adminRole = "Admin";
                if (!await roleMgr.RoleExistsAsync(adminRole))
                    await roleMgr.CreateAsync(new ApplicationRole { Id = Guid.NewGuid(), Name = adminRole, NormalizedName = adminRole.ToUpperInvariant() });

                var adminEmail = "admin@minya.gov.eg";
                var admin = await userMgr.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        Email = adminEmail,
                        UserName = adminEmail,
                        DisplayName = "System Admin",
                        EmailConfirmed = true
                    };
                    await userMgr.CreateAsync(admin, "Aa@1234"); // change in production
                    await userMgr.AddToRoleAsync(admin, adminRole);
                }
            }




            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();   // <-- IMPORTANT: before Authorization
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
