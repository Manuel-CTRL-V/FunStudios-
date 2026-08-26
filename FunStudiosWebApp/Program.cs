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
builder.Services.AddDbContext<FunStudiosContext>(options =>
    options.UseInMemoryDatabase("FunStudiosDb"));

// Servicios de negocio
builder.Services.AddScoped<IBiometriaService, SimuladorBiometriaService>();
builder.Services.AddScoped<IAccesoService, AccesoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

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

// Siembra inicial (catálogos + datos demo)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FunStudiosContext>();
    await FunStudiosSeeder.InicializarAsync(context);
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
