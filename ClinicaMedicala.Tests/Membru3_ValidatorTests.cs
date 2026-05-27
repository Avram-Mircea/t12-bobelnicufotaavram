using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Resurse;
using ClinicaMedicala.Services.Validare;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicaMedicala.Tests;

// ═══════════════════════════════════════════════════════════════════════════
//  MEMBRU 3 — Validator constrângeri programare (partea 1)
//  4 teste mapate la REQ-13..REQ-19 (asistent obligatoriu, mentenanță,
//  compatibilitate resursă-specializare, flow solicitare pacient).
//
//  Strategie: ConstraintValidationService folosește ApplicationDbContext +
//  IReguliConsultatieService. Pentru ctx folosim EFCore InMemory cu seed,
//  pentru reguli folosim Mock<IReguliConsultatieService>.
// ═══════════════════════════════════════════════════════════════════════════
public class Membru3_ValidatorTests
{
    // Helper: ctx InMemory + seed cu Medic + Asistent + Resursa + Specializari
    private static ApplicationDbContext CreeazaContextCuSeed(
        out int medicId, out int asistentId, out int resursaCardiologieId,
        out int resursaInMentenantaId)
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
            Specializare = "Cardiologie",     // ← cheia pentru REQ-15
            CodParafa = "ABC123", GradProfesional = GradProfesional.Specialist,
            CostConsultatie = 150m
        };
        var asistent = new Asistent
        {
            Nume = "Marin", Prenume = "Elena", Email = "elena@x.ro",
            Telefon = "0700000002", Adresa = "Adr 2", ParolaHash = "h",
            Rol = Rol.Asistent, StatusCont = true,
            Departament = "Cardiologie", Tura = Tura.Dimineata
        };
        var admin = new Administrator
        {
            Nume = "Sef", Prenume = "Admin", Email = "admin@x.ro",
            Telefon = "0700000003", Adresa = "Adr 3", ParolaHash = "h",
            Rol = Rol.Admin, StatusCont = true
        };

        var specCardiologie = new Specializare { Nume = "Cardiologie", Activ = true };
        var specPediatrie = new Specializare { Nume = "Pediatrie", Activ = true };

        // Resursă restricționată la Cardiologie — compatibilă cu medicul nostru
        var resursaCardiologie = new Resursa
        {
            Denumire = "Aparat EKG", Tip = TipResursa.Aparat_Imagistica,
            Stare = StareResursa.Functional, Activ = true,
            NumarInventar = "INV-EKG-01",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            Administrator = admin,
            Specializari = new List<Specializare> { specCardiologie }
        };

        // Resursă restricționată la Pediatrie — incompatibilă cu medicul de Cardiologie
        var resursaPediatrie = new Resursa
        {
            Denumire = "Sală pediatrie", Tip = TipResursa.Cabinet,
            Stare = StareResursa.Functional, Activ = true,
            NumarInventar = "INV-PED-01",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            Administrator = admin,
            Specializari = new List<Specializare> { specPediatrie }
        };

        // Resursă în mentenanță în intervalul testului (1–10 iunie 2026)
        var resursaMentenanta = new Resursa
        {
            Denumire = "RMN", Tip = TipResursa.Aparat_Imagistica,
            Stare = StareResursa.Functional, Activ = true,
            NumarInventar = "INV-RMN-01",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            Administrator = admin,
            PerioadeMentenanta = new List<PerioadaMentenanta>
            {
                new()
                {
                    Inceput = new DateTime(2026, 6, 1),
                    Sfarsit = new DateTime(2026, 6, 10),
                    Descriere = "Revizie planificată"
                }
            }
        };

        ctx.Medici.Add(medic);
        ctx.Asistenti.Add(asistent);
        ctx.Administratori.Add(admin);
        ctx.Specializari.AddRange(specCardiologie, specPediatrie);
        ctx.Resurse.AddRange(resursaCardiologie, resursaPediatrie, resursaMentenanta);
        ctx.SaveChanges();

        medicId = medic.Id;
        asistentId = asistent.Id;
        resursaCardiologieId = resursaCardiologie.Id;
        resursaInMentenantaId = resursaMentenanta.Id;

        // Pentru testul de incompatibilitate avem nevoie de Id-ul pediatriei
        // — îl returnăm prin variabilă statică ad-hoc (păstrăm semnătura simplă).
        IdResursaPediatrieUltimContext = resursaPediatrie.Id;
        return ctx;
    }
    private static int IdResursaPediatrieUltimContext;

    // ── Test 1 — REQ-17: tip „Procedura” cere asistent — fără el, eșec ─────
    [Fact]
    public async Task Validator_TipProceduraFaraAsistent_RespingeProgramarea()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out _, out _, out _);

        var mockReguli = new Mock<IReguliConsultatieService>();
        mockReguli.Setup(r => r.NecesitaAsistentAsync(TipProgramare.Procedura))
            .ReturnsAsync(true);

        var validator = new ConstraintValidationService(ctx, mockReguli.Object);

        var cerere = new CerereValidareProgramare
        {
            MedicId = medicId,
            TipProgramare = TipProgramare.Procedura,
            AsistentId = null,                            // ← NU atașăm asistent
            DataStart = new DateTime(2026, 7, 1, 10, 0, 0),
            DataEnd = new DateTime(2026, 7, 1, 10, 30, 0),
            EsteSolicitareDePacient = false               // creată de staff
        };

        // ACT
        var rezultat = await validator.ValideazaProgramareAsync(cerere);

        // ASSERT
        Assert.False(rezultat.EValida);
        Assert.Contains(rezultat.Erori, e => e.Contains("asistent", StringComparison.OrdinalIgnoreCase));
    }

    // ── Test 2 — REQ-17: solicitare de PACIENT — asistent atașat la confirmare ─
    // Pacientul nu poate atașa asistent — validatorul trebuie să accepte
    // solicitarea inițială cu AsistentId=null când EsteSolicitareDePacient=true.
    [Fact]
    public async Task Validator_TipProceduraDePacient_FaraAsistent_AccepatataPentruConfirmareUlterioara()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out _, out _, out _);

        var mockReguli = new Mock<IReguliConsultatieService>();
        mockReguli.Setup(r => r.NecesitaAsistentAsync(TipProgramare.Procedura))
            .ReturnsAsync(true);

        var validator = new ConstraintValidationService(ctx, mockReguli.Object);

        var cerere = new CerereValidareProgramare
        {
            MedicId = medicId,
            TipProgramare = TipProgramare.Procedura,
            AsistentId = null,
            DataStart = new DateTime(2026, 7, 1, 10, 0, 0),
            DataEnd = new DateTime(2026, 7, 1, 10, 30, 0),
            EsteSolicitareDePacient = true                // ← cheie: pacient
        };

        // ACT
        var rezultat = await validator.ValideazaProgramareAsync(cerere);

        // ASSERT — nicio eroare legată de asistent
        Assert.DoesNotContain(rezultat.Erori,
            e => e.Contains("asistent", StringComparison.OrdinalIgnoreCase));
    }

    // ── Test 3 — REQ-14, REQ-15: resursă în mentenanță în interval — eșec ──
    [Fact]
    public async Task Validator_ResursaInMentenantaInInterval_RespingeProgramarea()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(
            out var medicId, out _, out _, out var resursaMentenantaId);

        var mockReguli = new Mock<IReguliConsultatieService>();
        mockReguli.Setup(r => r.NecesitaAsistentAsync(It.IsAny<TipProgramare>()))
            .ReturnsAsync(false);

        var validator = new ConstraintValidationService(ctx, mockReguli.Object);

        // Programare în 5 iunie 2026 — fix în mijlocul perioadei de mentenanță (1–10 iunie)
        var cerere = new CerereValidareProgramare
        {
            MedicId = medicId,
            TipProgramare = TipProgramare.Consult_Initial,
            ResursaIds = new List<int> { resursaMentenantaId },
            DataStart = new DateTime(2026, 6, 5, 10, 0, 0),
            DataEnd = new DateTime(2026, 6, 5, 11, 0, 0)
        };

        // ACT
        var rezultat = await validator.ValideazaProgramareAsync(cerere);

        // ASSERT
        Assert.False(rezultat.EValida);
        Assert.Contains(rezultat.Erori, e => e.Contains("mentenanță", StringComparison.OrdinalIgnoreCase));
    }

    // ── Test 4 — REQ-13, REQ-15: resursă restricționată pe altă specializare ─
    // Medicul are specializarea „Cardiologie", dar încearcă să folosească
    // o resursă restricționată la „Pediatrie" → respins cu mesaj clar.
    [Fact]
    public async Task Validator_ResursaIncompatibilaCuSpecializareaMedicului_RespingeProgramarea()
    {
        // ARRANGE
        using var ctx = CreeazaContextCuSeed(out var medicId, out _, out _, out _);
        var resursaPediatrieId = IdResursaPediatrieUltimContext;

        var mockReguli = new Mock<IReguliConsultatieService>();
        mockReguli.Setup(r => r.NecesitaAsistentAsync(It.IsAny<TipProgramare>()))
            .ReturnsAsync(false);

        var validator = new ConstraintValidationService(ctx, mockReguli.Object);

        var cerere = new CerereValidareProgramare
        {
            MedicId = medicId,                                  // Cardiologie
            TipProgramare = TipProgramare.Consult_Initial,
            ResursaIds = new List<int> { resursaPediatrieId },  // Pediatrie
            DataStart = new DateTime(2026, 7, 1, 10, 0, 0),
            DataEnd = new DateTime(2026, 7, 1, 10, 30, 0)
        };

        // ACT
        var rezultat = await validator.ValideazaProgramareAsync(cerere);

        // ASSERT
        Assert.False(rezultat.EValida);
        Assert.Contains(rezultat.Erori, e =>
            e.Contains("Pediatrie", StringComparison.OrdinalIgnoreCase) &&
            e.Contains("Cardiologie", StringComparison.OrdinalIgnoreCase));
    }
}
