using System.Globalization;
using FunStudiosWebApp.Data;
using FunStudiosWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cultura de la aplicación DOP (peso dominicano, $)
var culturaDO = new CultureInfo("es-DO");
culturaDO.NumberFormat.CurrencySymbol = "$";
CultureInfo.DefaultThreadCurrentCulture = culturaDO;
CultureInfo.DefaultThreadCurrentUICulture = culturaDO;

// MVC
builder.Services.AddControllersWithViews();

// Base de datos
// Por ahora se usa el proveedor InMemory para mimiquear el comportamiento
// completo del sistema sin SQL Server. Al migrar a producción solo cambiamos
// esta línea por UseSqlServer con la cadena de conexión de appsettings.json.
//builder.Services.AddDbContext<FunStudiosContext>(options =>
//    options.UseInMemoryDatabase("FunStudiosDb"));

// Base de datos — SQL Server
builder.Services.AddDbContext<FunStudiosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FunStudiosDb")));

// Servicios de negocio
// El lector biométrico vive en la PC de recepción; la web lo consume por red
// mediante la API local de BiometricService (config BiometriaApi:BaseUrl).
builder.Services.AddHttpClient<ApiBiometriaService>(client =>
{
    var baseUrl = builder.Configuration["BiometriaApi:BaseUrl"] ?? "http://localhost:5050";
    var timeout = builder.Configuration.GetValue<int>("BiometriaApi:TimeoutSegundos", 90);
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(timeout);
});
builder.Services.AddScoped<IBiometriaService>(sp => sp.GetRequiredService<ApiBiometriaService>());

builder.Services.AddScoped<IAccesoService, AccesoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IFamiliaService, FamiliaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

// Autenticación por cookies con roles (RF11)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FunStudios.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

// Siembra inicial (catálogos + admin; datos demo solo en desarrollo)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FunStudiosContext>();
    var incluirDemo = app.Configuration.GetValue<bool?>("Seed:DemoData")
        ?? app.Environment.IsDevelopment();
    await FunStudiosSeeder.InicializarAsync(context, incluirDemo);
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// El orden importa: autenticación identifica al usuario, autorización valida su rol
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
