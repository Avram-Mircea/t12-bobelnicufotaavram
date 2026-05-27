using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services.Pacienti;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Tests;

// ═══════════════════════════════════════════════════════════════════════════
//  MEMBRU 3 — Rating (partea 2)
//  3 teste mapate la REQ-45, REQ-46 (rating pacient → medic, mediere, filtre).
//
//  Folosim RatingRepository real pe ApplicationDbContext InMemory ca să
//  acoperim efectiv LINQ-ul de filtrare (.Where(Vizibil && !AcordatDeMedic)).
// ═══════════════════════════════════════════════════════════════════════════
public class Membru3_RatingTests
{
    // Helper: ctx + utilizator/medic minimali pentru asocieri FK
    private static ApplicationDbContext CreeazaContextCuSeed(
        out int medicId, out int pacientId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var ctx = new ApplicationDbContext(options);

        var medic = new Medic
        {
            Nume = "Popescu", Prenume = "Ana", Email = "ana@x.ro",
            Telefon = "0700000001", Adresa = "Adr 1", ParolaHash = "h",
            Rol = Rol.Medic, StatusCont = true,
            Specializare = "Cardiologie", CodParafa = "ABC123",
            GradProfesional = GradProfesional.Specialist, CostConsultatie = 150m
        };
        var pacient = new Pacient
        {
            Nume = "Ionescu", Prenume = "Mihai", Email = "mihai@x.ro",
            Telefon = "0700000002", Adresa = "Adr 2", ParolaHash = "h",
            Rol = Rol.Pacient, StatusCont = true,
            CNP = "1900101225588", DataNastere = new DateTime(1990, 1, 1),
            GrupaSanguina = GrupaSanguina.O_Pozitiv,
            ContactUrgentaNume = "X", ContactUrgentaTelefon = "0700"
        };

        ctx.Medici.Add(medic);
        ctx.Pacienti.Add(pacient);
        ctx.SaveChanges();

        medicId = medic.Id;
        pacientId = pacient.Id;
        return ctx;
    }

    private static Rating CreeazaRating(
        int pacientId, int medicId, int scor,
        bool vizibil = true, bool acordatDeMedic = false)
        => new()
        {
            PacientId = pacientId,
            MedicId = medicId,
            Scor = scor,
            Vizibil = vizibil,
            AcordatDeMedic = acordatDeMedic,
            Data = DateTime.UtcNow
        };

    // ── Test 5 — REQ-46: lista publică NU include rating-uri medic → pacient ─
    // Dacă filtrarea pe `!AcordatDeMedic` se sparge, rating-urile interne
    // medic→pacient ar deveni publice. Test critic pentru confidențialitate.
    [Fact]
    public async Task RatingRepository_GetByMedicId_NuInclude_RatingMedicCatrePacient()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId);
        var repo = new RatingRepository(ctx);

        // Două rating-uri în DB: unul de la pacient, unul de la medic
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, scor: 5, acordatDeMedic: false));  // public
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, scor: 3, acordatDeMedic: true));   // intern
        await ctx.SaveChangesAsync();

        // ACT
        var ratinguriPublice = await repo.GetByMedicIdAsync(medicId);

        // ASSERT
        Assert.Single(ratinguriPublice);
        Assert.False(ratinguriPublice[0].AcordatDeMedic,
            "Rating-urile medic→pacient nu trebuie să apară în lista publică.");
        Assert.Equal(5, ratinguriPublice[0].Scor);
    }

    // ── Test 6 — edge case: medic fără rating-uri — media e 0 ──────────────
    // RatingService.GetAverage trebuie să nu arunce DivisionByZero pe
    // colecție goală — pattern clasic de bug în calcule de medii.
    [Fact]
    public async Task RatingService_GetAverage_FaraInregistrari_Returneaza0()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out _);
        var repo = new RatingRepository(ctx);
        var service = new RatingService(repo);

        // ACT
        var media = await service.GetAverageRatingForMedicAsync(medicId);

        // ASSERT
        Assert.Equal(0, media);
    }

    // ── Test 7 — REQ-46: media corectă din 3 note publice + ignoră ratingurile interne ──
    [Fact]
    public async Task RatingService_GetAverage_CuPublice_5_4_3_SiIntern1_ReturneazaMedia4()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId);
        var repo = new RatingRepository(ctx);
        var service = new RatingService(repo);

        // 3 rating-uri publice (5, 4, 3) → media 4.0
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, 5));
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, 4));
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, 3));

        // 1 rating intern medic→pacient (1) — TREBUIE ignorat
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, 1, acordatDeMedic: true));

        // 1 rating ascuns de admin (Vizibil=false) — TREBUIE ignorat
        ctx.Ratinguri.Add(CreeazaRating(pacientId, medicId, 1, vizibil: false));

        await ctx.SaveChangesAsync();

        // ACT
        var media = await service.GetAverageRatingForMedicAsync(medicId);

        // ASSERT
        Assert.Equal(4.0, media);
    }
}
