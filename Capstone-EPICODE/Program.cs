using Capstone_EPICODE.Data;
using Capstone_EPICODE.Services;
using Capstone_EPICODE.Services.Interfaces;
using Capstone_EPICODE.Services.Password;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Servizi personalizzati
builder.Services.AddScoped<IPasswordEnconder, PasswordEnconder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Connessione al database
var conn = builder.Configuration.GetConnectionString("SqlServer")!;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(conn));

// Configurazione autenticazione tramite cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";  // Percorso del login
        options.LogoutPath = "/Auth/Logout"; // Percorso del logout
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Abilita l'autenticazione
app.UseAuthorization(); // Abilita l'autorizzazione

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
