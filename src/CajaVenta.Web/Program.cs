using System.Security.Claims;
using CajaVenta.Infrastructure;
using CajaVenta.Infrastructure.Persistence;
using CajaVenta.Infrastructure.Seed;
using CajaVenta.Web.Middleware;
using CajaVenta.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CajaVenta");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "..", "CajaVenta.db");
    connectionString = $"Data Source={Path.GetFullPath(dbPath)}";
}

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.Name = ".CajaVenta.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.Name = ".CajaVenta.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(10);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<ILicenciaClienteService, LicenciaClienteService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LicenciaMiddleware>();

using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CajaVentaDbContext>();
        context.Database.EnsureCreated();
        EsquemaMigracion.Aplicar(context);
        SeedData.Inicializar(context, scope.ServiceProvider);
    }

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();