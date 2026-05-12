using ClinicaMedicala.Data;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Repositories.Implementations;
using ClinicaMedicala.Repositories.Interfaces;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Implementations;
using ClinicaMedicala.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Generic Repository & Service
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));

builder.Services.AddScoped<IProgramareRepository, ProgramareRepository>();
builder.Services.AddScoped<IProgramareService, ProgramareService>();

builder.Services.AddScoped<IFisaMedicalaService, FisaMedicalaService>();
builder.Services.AddScoped<IFisaMedicalaRepository, FisaMedicalaRepository>();

builder.Services.AddScoped<IDocumentMedicalService, DocumentMedicalService>();
builder.Services.AddScoped<IDocumentMedicalRepository, DocumentMedicalRepository>();

builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=medic}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
