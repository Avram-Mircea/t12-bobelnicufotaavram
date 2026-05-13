using ClinicaMedicala.Models;
using ClinicaMedicala.Services;

namespace ClinicaMedicala.Data;

// Seed inițial — creează un admin la primul start, altfel sistemul e blocat
// (nu există niciun cont, deci nimeni nu se poate autentifica).
public static class DbSeeder
{
    private const string AdminEmailImplicit = "admin@clinica.ro";
    private const string AdminParolaImplicita = "Admin123!";

    public static async Task EnsureAdminAsync(ApplicationDbContext context, IPasswordHasher hasher)
    {
        if (context.Administratori.Any()) return;

        var admin = new Administrator
        {
            Nume = "Sistem",
            Prenume = "Admin",
            Email = AdminEmailImplicit,
            Telefon = "0000000000",
            Adresa = "—",
            Rol = Rol.Admin,
            StatusCont = true,
            DataCreareCont = DateTime.UtcNow,
            ParolaHash = hasher.Hash(AdminParolaImplicita)
        };

        context.Administratori.Add(admin);
        await context.SaveChangesAsync();
    }
}
