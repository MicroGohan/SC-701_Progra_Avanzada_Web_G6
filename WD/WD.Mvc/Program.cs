using Microsoft.EntityFrameworkCore;
using WD.Data.DB;
using WD.Mvc.Services;
using WD.Repository.Interfaces;
using WD.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WeatherDB")));

builder.Services.AddHttpClient<WeatherApiClient>(client =>
{
    var envBase = Environment.GetEnvironmentVariable("WEATHER_API_BASEURL");
    var baseUrl = string.IsNullOrWhiteSpace(envBase) ? "https://localhost:7215/" : envBase;
    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
});

// Repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();

// Servicios
builder.Services.AddSession();
builder.Services.AddScoped<FavoritosService>();   
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PublicTopService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<SettingsAppService>(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=SignUp}/{id?}");

app.Run();
