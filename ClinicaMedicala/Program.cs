using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Resurse;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── EF Core ───────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUtilizatorRepository, UtilizatorRepository>();
builder.Services.AddScoped<IAutentificareRepository, AutentificareRepository>();
builder.Services.AddScoped<IResursaRepository, ResursaRepository>();

// ── Servicii ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUtilizatorService, UtilizatorService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IResursaService, ResursaService>();

// ── Authentication: Cookie-based ──────────────────────────────────────────────
// REQ-01, REQ-03: rol stocat în claim, un singur rol principal per utilizator
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ClinicaMedicala.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";

        // 8 ore — durată rezonabilă pentru o tură de lucru într-o clinică
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ── Authorization: policies per rol ───────────────────────────────────────────
// REQ-04, REQ-05: restricționare acces pe baza rolului
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PoliciiAuth.DoarAdmin, p => p.RequireRole(Rol.Admin.ToString()));
    options.AddPolicy(PoliciiAuth.DoarMedic, p => p.RequireRole(Rol.Medic.ToString()));
    options.AddPolicy(PoliciiAuth.DoarAsistent, p => p.RequireRole(Rol.Asistent.ToString()));
    options.AddPolicy(PoliciiAuth.DoarPacient, p => p.RequireRole(Rol.Pacient.ToString()));

    // Policies combinate pentru endpoint-uri partajate
    options.AddPolicy(PoliciiAuth.StaffClinica, p =>
        p.RequireRole(Rol.Admin.ToString(), Rol.Medic.ToString(), Rol.Asistent.ToString()));

    options.AddPolicy(PoliciiAuth.AdminSauMedic, p =>
        p.RequireRole(Rol.Admin.ToString(), Rol.Medic.ToString()));
});

builder.Services.AddHttpContextAccessor();

// ── Data Protection ───────────────────────────────────────────────────────────
// În Development, keys-urile sunt EFEMERE (în memorie) — la fiecare restart
// al serverului, cookie-urile vechi devin invalide și utilizatorii sunt
// redirecționați la pagina de Login.
// În Production, ASP.NET persistă cheile pe disc, deci utilizatorii rămân
// logați și după deploy-uri (comportament dorit în producție).
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDataProtectionProvider>(sp =>
        new EphemeralDataProtectionProvider(sp.GetService<ILoggerFactory>()));
}

var app = builder.Build();

// Seed: garantăm un admin la primul start, altfel nimeni nu se poate autentifica
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.EnsureAdminAsync(ctx, hasher);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Ordinea contează: Authentication ÎNAINTE de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
