using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services.Resurse;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicaMedicala.Tests;

// ═══════════════════════════════════════════════════════════════════════════
//  MEMBRU 1 — Gestionarea Resurselor
//  3 teste mapate la REQ-08..REQ-14 (resurse, mentenanță, unicitate).
// ═══════════════════════════════════════════════════════════════════════════
public class Membru1_ResurseTests
{
    // Helper: creează un ApplicationDbContext pe EFCore InMemory — izolat per test.
    private static ApplicationDbContext CreeazaContextInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    // ── Test 5 — REQ-11/12: dezactivarea unei resurse active funcționează ──
    [Fact]
    public async Task ResursaService_Dezactiveaza_CandResursaActiva_ReturneazaTrueSiMarcheazaInactiva()
    {
        // ARRANGE
        var resursa = new Resursa
        {
            Id = 1,
            Denumire = "Cabinet 101",
            Tip = TipResursa.Cabinet,
            Stare = StareResursa.Functional,
            Activ = true,
            NumarInventar = "INV-001",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            AdministratorId = 1
        };

        var mockRepo = new Mock<IResursaRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(resursa);
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        using var ctx = CreeazaContextInMemory();
        var service = new ResursaService(mockRepo.Object, ctx);

        // ACT
        var rezultat = await service.DezactiveazaAsync(1);

        // ASSERT
        Assert.True(rezultat, "Dezactivarea unei resurse active trebuie să întoarcă true.");
        Assert.False(resursa.Activ, "Câmpul Activ trebuie pus pe false după dezactivare.");
        mockRepo.Verify(r => r.Update(resursa), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── Test 6 — REQ-11: dezactivarea unei resurse deja inactive nu face nimic ──
    [Fact]
    public async Task ResursaService_Dezactiveaza_CandResursaDejaInactiva_ReturneazaFalse()
    {
        // ARRANGE
        var resursaInactiva = new Resursa
        {
            Id = 2,
            Denumire = "Cabinet 202",
            Tip = TipResursa.Cabinet,
            Stare = StareResursa.Functional,
            Activ = false,   // deja dezactivată
            NumarInventar = "INV-002",
            DataUltimaRevizie = DateTime.UtcNow.AddMonths(-2),
            DataScadentaRevizie = DateTime.UtcNow.AddMonths(10),
            AdministratorId = 1
        };

        var mockRepo = new Mock<IResursaRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(resursaInactiva);

        using var ctx = CreeazaContextInMemory();
        var service = new ResursaService(mockRepo.Object, ctx);

        // ACT
        var rezultat = await service.DezactiveazaAsync(2);

        // ASSERT
        Assert.False(rezultat, "Dezactivarea unei resurse deja inactive trebuie să întoarcă false.");
        mockRepo.Verify(r => r.Update(It.IsAny<Resursa>()), Times.Never,
            "Nu trebuie să se apeleze Update dacă resursa era deja inactivă.");
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ── Test 7 — REQ-13: nume specializare duplicat e respins ───────────────
    // SpecializareService.CreeazaAsync verifică unicitatea numelui ÎNAINTE de
    // a apela DB-ul, ca să returneze un mesaj prietenos (nu DbUpdateException).
    [Fact]
    public async Task SpecializareService_CreeazaCuNumeDuplicat_ArunaInvalidOperationException()
    {
        // ARRANGE
        var mockRepo = new Mock<ISpecializareRepository>();
        mockRepo.Setup(r => r.NumeExistaAsync("Cardiologie"))
            .ReturnsAsync(true);   // simulăm că deja există

        var service = new SpecializareService(mockRepo.Object);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreeazaAsync("Cardiologie", descriere: null));

        Assert.Contains("Cardiologie", ex.Message);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Specializare>()), Times.Never,
            "Nu trebuie să se apeleze AddAsync dacă numele e deja folosit.");
    }
}
