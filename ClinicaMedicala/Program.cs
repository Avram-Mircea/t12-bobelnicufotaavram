using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Auth;
using ClinicaMedicala.Services.Pacienti;
using ClinicaMedicala.Services.Programari;
using ClinicaMedicala.Services.Resurse;
using ClinicaMedicala.Services.Validare;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Cultură (ro-RO) ───────────────────────────────────────────────────────────
// Setăm cultura aplicației pe ro-RO, ca model binder-ul să parseze date
// în format dd/MM/yyyy (introduse manual de utilizator) indiferent de OS.
// Suprascriem ShortDatePattern ca să folosim "/" în loc de "." — separatorul
// e mai vizibil în input-uri și e auto-completat de JS pe măsură ce se tastează.
var culturaRo = new CultureInfo("ro-RO");
culturaRo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
culturaRo.DateTimeFormat.DateSeparator = "/";
CultureInfo.DefaultThreadCurrentCulture = culturaRo;
CultureInfo.DefaultThreadCurrentUICulture = culturaRo;

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culturaRo);
    options.SupportedCultures = new[] { culturaRo };
    options.SupportedUICultures = new[] { culturaRo };
});

// ── EF Core ───────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUtilizatorRepository, UtilizatorRepository>();
builder.Services.AddScoped<IAutentificareRepository, AutentificareRepository>();
builder.Services.AddScoped<IResursaRepository, ResursaRepository>();
builder.Services.AddScoped<ISpecializareRepository, SpecializareRepository>();
builder.Services.AddScoped<IReguliConsultatieRepository, ReguliConsultatieRepository>();
builder.Services.AddScoped<IProgramareRepository, ProgramareRepository>();
builder.Services.AddScoped<IFisaMedicalaRepository, FisaMedicalaRepository>();
builder.Services.AddScoped<IDocumentMedicalRepository, DocumentMedicalRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

// ── Servicii ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUtilizatorService, UtilizatorService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IResursaService, ResursaService>();
builder.Services.AddScoped<ISpecializareService, SpecializareService>();
builder.Services.AddScoped<IReguliConsultatieService, ReguliConsultatieService>();
builder.Services.AddScoped<IConstraintValidationService, ConstraintValidationService>();
builder.Services.AddScoped<IProgramareService, ProgramareService>();
builder.Services.AddScoped<IFisaMedicalaService, FisaMedicalaService>();
builder.Services.AddScoped<IDocumentMedicalService, DocumentMedicalService>();
builder.Services.AddScoped<IRatingService, RatingService>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();

// Ordinea contează: Authentication ÎNAINTE de Authorization
app.UseAuthentication();
app.UseAuthorization();

// First-run gate: dacă nu există niciun utilizator în baza de date,
// redirecționăm orice request către /Setup, ca primul cont creat să fie
// administrator (introdus manual de utilizator, fără credențiale implicite).
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? string.Empty;

    // Permitem fișierele statice, endpoint-urile de Setup și asset-urile vendor
    // să treacă liber, ca pagina să se poată afișa corect.
    bool eExclus =
        path.StartsWith("/Setup", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);

    if (!eExclus)
    {
        var db = ctx.RequestServices.GetRequiredService<ApplicationDbContext>();
        if (!await db.Utilizatori.AnyAsync())
        {
            ctx.Response.Redirect("/Setup");
            return;
        }
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
