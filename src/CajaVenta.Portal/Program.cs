using CajaVenta.Portal.Data;
using CajaVenta.Portal.Services;
using CajaVenta.Portal.Services.Pasarelas;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var rutaDatos = Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(rutaDatos);
var conexionPortal = $"Data Source={Path.Combine(rutaDatos, "portal.db")}";

builder.Services.AddDbContext<PortalDbContext>(options => options.UseSqlite(conexionPortal));

builder.Services.AddScoped<ISuscriptorService, SuscriptorService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISuscripcionService, SuscripcionService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<ILicenciaService, LicenciaService>();
builder.Services.AddScoped<ITenantProvisioner, TenantProvisioner>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Pasarela");
builder.Services.AddHttpClient();

builder.Services.AddScoped<PasarelaFactory>();
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<PasarelaFactory>();
    return factory.Crear();
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.Name = ".CajaVenta.Portal.Session";
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
        options.Cookie.Name = ".CajaVenta.Portal.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
    context.Database.EnsureCreated();
    EsquemaPortal.Sembrar(context);
}

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Licencia}/{action=Index}/{id?}");

app.Run();