using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// ── Servicii ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUtilizatorService, UtilizatorService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

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

var app = builder.Build();

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
