using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services.Programari;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Tests;

// ═══════════════════════════════════════════════════════════════════════════
//  MEMBRU 2 — Programări, Calendar, Suprapuneri
//  7 teste mapate la REQ-22, REQ-23, REQ-27, REQ-28..32, REQ-33, REQ-49.
//
//  Strategie: folosim ProgramareRepository REAL pe un ApplicationDbContext
//  InMemory — testăm efectiv interogările LINQ (suprapuneri, filtre,
//  status), nu doar contractul cu mock-uri triviale.
// ═══════════════════════════════════════════════════════════════════════════
public class Membru2_ProgramariTests
{
    // ── Helper: context InMemory + seed cu un medic/pacient minimal ────────
    private static ApplicationDbContext CreeazaContextCuSeed(
        out int medicId, out int pacientId, out int resursaId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var ctx = new ApplicationDbContext(options);

        var medic = new Medic
        {
            Nume = "Popescu", Prenume = "Ana",
            Email = "ana@clinica.ro", Telefon = "0700000001", Adresa = "Str. Test 1",
            ParolaHash = "h", Rol = Rol.Medic, StatusCont = true,
            Specializare = "Cardiologie", CodParafa = "ABC123",
            GradProfesional = GradProfesional.Specialist, CostConsultatie = 150m
        };

        var pacient = new Pacient
        {
            Nume = "Ionescu", Prenume = "Mihai",
            Email = "mihai@x.ro", Telefon = "0700000002", Adresa = "Str. Test 2",
            ParolaHash = "h", Rol = Rol.Pacient, StatusCont = true,
            CNP = "1900101225588",
            DataNastere = new DateTime(1990, 1, 1),
            GrupaSanguina = GrupaSanguina.O_Pozitiv,
            ContactUrgentaNume = "Ionescu Maria",
            ContactUrgentaTelefon = "0711111111"
        };

        var admin = new Administrator
        {
            Nume = "Admin", Prenume = "Sef",
            Email = "admin@x.ro", Telefon = "0700000003", Adresa = "Str. Test 3",
            ParolaHash = "h", Rol = Rol.Admin, StatusCont = true
        };

        var resursa = new Resursa
        {
            Denumire = "Cabinet 101",
            Tip = TipResursa.Cabinet,
            Stare = StareResursa.Functional,
            Activ = true,
            NumarInventar = "INV-001",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            Administrator = admin
        };

        ctx.Medici.Add(medic);
        ctx.Pacienti.Add(pacient);
        ctx.Administratori.Add(admin);
        ctx.Resurse.Add(resursa);
        ctx.SaveChanges();

        medicId = medic.Id;
        pacientId = pacient.Id;
        resursaId = resursa.Id;
        return ctx;
    }

    // Helper: creează o programare cu setări minimale.
    private static Programare CreeazaProgramare(
        int medicId, int pacientId, DateTime start, DateTime end,
        StatusProgramare status = StatusProgramare.Confirmat,
        int? resursaId = null)
    {
        return new Programare
        {
            MedicId = medicId,
            PacientId = pacientId,
            DataStart = start,
            DataEnd = end,
            Status = status,
            ResursaId = resursaId,
            MotivVizita = "Test",
            TipProgramare = TipProgramare.Consult_Initial
        };
    }

    // ── Test 1 — REQ-22: medic liber în interval gol ───────────────────────
    // Happy path: nu există nicio programare a medicului → fereastra cerută
    // e disponibilă → MedicEsteDisponibil = true.
    [Fact]
    public async Task MedicEsteDisponibil_FaraProgramariExistente_ReturneazaTrue()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out _, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        var start = new DateTime(2026, 6, 1, 10, 0, 0);
        var end = start.AddMinutes(30);

        // ACT
        var disponibil = await service.MedicEsteDisponibilAsync(medicId, start, end);

        // ASSERT
        Assert.True(disponibil, "Medic fără programări în interval trebuie să fie disponibil.");
    }

    // ── Test 2 — REQ-22, REQ-27: detecție suprapunere medic ────────────────
    // Critic pentru integritatea calendarului: dacă două programări ar putea
    // exista simultan pentru același medic, se prăbușește planificarea.
    [Fact]
    public async Task MedicEsteDisponibil_CuProgramareSuprapusa_ReturneazaFalse()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        // Programare existentă: 10:00 — 10:30
        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 10, 30, 0)));
        await ctx.SaveChangesAsync();

        // Cerere de programare nouă: 10:15 — 10:45 (suprapunere parțială)
        var startNou = new DateTime(2026, 6, 1, 10, 15, 0);
        var endNou = new DateTime(2026, 6, 1, 10, 45, 0);

        // ACT
        var disponibil = await service.MedicEsteDisponibilAsync(medicId, startNou, endNou);

        // ASSERT
        Assert.False(disponibil,
            "Suprapunere parțială cu programare existentă — medicul NU trebuie să fie disponibil.");
    }

    // ── Test 3 — REQ-22: programări anulate nu blochează intervalul ────────
    // Bug clasic: dacă verificarea de suprapunere include și programările
    // anulate (Anulat_Pacient / Anulat_Clinica), sloturile rămân blocate
    // chiar dacă pacientul a anulat. Asta trebuie testat explicit.
    [Fact]
    public async Task MedicEsteDisponibil_CuProgramareAnulata_O_Ignora()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        // Programare ANULATĂ pe slot-ul 10:00 — 10:30
        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 10, 30, 0),
            status: StatusProgramare.Anulat_Pacient));
        await ctx.SaveChangesAsync();

        var startNou = new DateTime(2026, 6, 1, 10, 0, 0);
        var endNou = new DateTime(2026, 6, 1, 10, 30, 0);

        // ACT
        var disponibil = await service.MedicEsteDisponibilAsync(medicId, startNou, endNou);

        // ASSERT
        Assert.True(disponibil,
            "O programare ANULATĂ nu trebuie să mai blocheze slotul — medicul rămâne disponibil.");
    }

    // ── Test 4 — REQ-22: la EDIT, programarea curentă nu se suprapune cu ea însăși ──
    // Când modifici o programare existentă (ora ei rămâne aceeași), verificarea
    // de suprapunere trebuie să o excludă din căutare — altfel nu vei putea
    // niciodată salva edit-urile minore (ex: schimbat doar motivul vizitei).
    [Fact]
    public async Task MedicEsteDisponibil_LaEdit_ExcludePropriaProgramare_ReturneazaTrue()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        var programareExistenta = CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 6, 1, 10, 0, 0),
            new DateTime(2026, 6, 1, 10, 30, 0));
        ctx.Programari.Add(programareExistenta);
        await ctx.SaveChangesAsync();

        // ACT — re-validăm fix același interval, excluzând Id-ul curent
        var disponibil = await service.MedicEsteDisponibilAsync(
            medicId,
            programareExistenta.DataStart,
            programareExistenta.DataEnd,
            programareExclusaId: programareExistenta.Id);

        // ASSERT
        Assert.True(disponibil,
            "Editarea propriei programări nu trebuie să detecteze suprapunere cu ea însăși.");
    }

    // ── Test 5 — REQ-23, REQ-27: suprapunere pe RESURSĂ ────────────────────
    // Două programări nu pot folosi același cabinet/aparat în paralel.
    [Fact]
    public async Task ResursaEsteDisponibila_CuProgramareSuprapusa_ReturneazaFalse()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out var resursaId);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        // Cabinet ocupat 14:00 — 15:00
        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 6, 1, 14, 0, 0),
            new DateTime(2026, 6, 1, 15, 0, 0),
            resursaId: resursaId));
        await ctx.SaveChangesAsync();

        // Cerere: 14:30 — 15:30 (suprapunere de 30 min)
        var startNou = new DateTime(2026, 6, 1, 14, 30, 0);
        var endNou = new DateTime(2026, 6, 1, 15, 30, 0);

        // ACT
        var disponibil = await service.ResursaEsteDisponibilaAsync(resursaId, startNou, endNou);

        // ASSERT
        Assert.False(disponibil,
            "Suprapunere pe aceeași resursă — al doilea slot trebuie respins.");
    }

    // ── Test 6 — REQ-49: rating doar după consultație FINALIZATĂ ───────────
    // Pacientul nu poate da rating dacă nu a fost văzut efectiv de medic —
    // statusul programării e sursa de adevăr.
    [Fact]
    public async Task HasCompletedConsultation_CuStatusFinalizat_ReturneazaTrue()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 4, 1, 10, 0, 0),
            new DateTime(2026, 4, 1, 10, 30, 0),
            status: StatusProgramare.Finalizat));
        await ctx.SaveChangesAsync();

        // ACT
        var areConsultatieFinalizata = await service.HasCompletedConsultationAsync(pacientId, medicId);

        // ASSERT
        Assert.True(areConsultatieFinalizata,
            "Pacientul cu o programare Finalizat trebuie să aibă drept de rating.");
    }

    // ── Test 7 — REQ-49: programări doar CONFIRMATE nu deschid rating-ul ───
    // Caz negativ: doar Confirmat nu e suficient — pacientul ar putea da
    // rating înainte ca consultația să fi avut loc, ceea ce nu vrem.
    [Fact]
    public async Task HasCompletedConsultation_CuStatusConfirmatDarNeFinalizat_ReturneazaFalse()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out var pacientId, out _);
        var repo = new ProgramareRepository(ctx);
        var service = new ProgramareService(repo);

        // Două programări: una Programat, una Confirmat — NICIUNA Finalizat
        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 4, 1, 10, 0, 0),
            new DateTime(2026, 4, 1, 10, 30, 0),
            status: StatusProgramare.Programat));

        ctx.Programari.Add(CreeazaProgramare(
            medicId, pacientId,
            new DateTime(2026, 5, 1, 10, 0, 0),
            new DateTime(2026, 5, 1, 10, 30, 0),
            status: StatusProgramare.Confirmat));

        await ctx.SaveChangesAsync();

        // ACT
        var areConsultatieFinalizata = await service.HasCompletedConsultationAsync(pacientId, medicId);

        // ASSERT
        Assert.False(areConsultatieFinalizata,
            "Fără nicio programare Finalizat, pacientul nu trebuie să poată acorda rating.");
    }
}
