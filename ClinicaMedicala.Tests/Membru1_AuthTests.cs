using ClinicaMedicala.Models;
using ClinicaMedicala.Models.Validation;
using ClinicaMedicala.Repositories;
using ClinicaMedicala.Services;
using ClinicaMedicala.Services.Auth;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ClinicaMedicala.Tests;

// ═══════════════════════════════════════════════════════════════════════════
//  MEMBRU 1 — Autentificare / Login / Înregistrare
//  4 teste mapate la REQ-01..REQ-07 (autentificare, hash parolă, validare).
// ═══════════════════════════════════════════════════════════════════════════
public class Membru1_AuthTests
{
    // ── Test 1 — REQ-04: validare parolă (politică de complexitate) ─────────
    // Verifică: o parolă „normală” cu min 8 caractere + cel puțin un simbol
    // este acceptată de StrongPasswordAttribute.
    [Fact]
    public void StrongPassword_CuSimbolSiPesteMinim_EsteValida()
    {
        // ARRANGE
        var attribut = new StrongPasswordAttribute();
        var parolaConforma = "Parola123!";

        // ACT
        var rezultat = attribut.IsValid(parolaConforma);

        // ASSERT
        Assert.True(rezultat, "O parolă cu minim 8 caractere și un simbol trebuie acceptată.");
    }

    // ── Test 2 — REQ-04: validare parolă — caz negativ ──────────────────────
    // Verifică: o parolă doar alfanumerică (fără simbol) e respinsă.
    [Fact]
    public void StrongPassword_FaraCaractereSpeciale_EsteInvalida()
    {
        // ARRANGE
        var attribut = new StrongPasswordAttribute();
        var parolaSlaba = "Parola123";   // fără ! @ # etc.

        // ACT
        var rezultat = attribut.IsValid(parolaSlaba);

        // ASSERT
        Assert.False(rezultat,
            "O parolă fără caractere speciale trebuie respinsă (politica de complexitate).");
    }

    // ── Test 3 — REQ-04: hash + verificare BCrypt round-trip ────────────────
    // Verifică că o parolă hash-uită cu BCrypt poate fi verificată corect
    // și că hash-ul nu este parola în clar.
    [Fact]
    public void BCryptPasswordHasher_HashSiVerify_ParolaCorecta_ReturneazaTrue()
    {
        // ARRANGE
        var hasher = new BCryptPasswordHasher();
        var parolaInClar = "ParolaUtilizator123!";

        // ACT
        var hash = hasher.Hash(parolaInClar);
        var verificare = hasher.Verify(parolaInClar, hash);

        // ASSERT
        Assert.NotEqual(parolaInClar, hash);                  // nu stocăm plain text
        Assert.StartsWith("$2", hash);                         // formatul BCrypt
        Assert.True(verificare, "Verify cu parola corectă trebuie să întoarcă true.");
    }

    // ── Test 4 — REQ-01: login cu parolă greșită ────────────────────────────
    // AuthService.LoginAsync trebuie să respingă o parolă greșită cu mesaj
    // standard „Email sau parolă incorectă.” (nu divulgăm DE CE a eșuat).
    [Fact]
    public async Task AuthService_LoginCuParolaGresita_ReturneazaEsec()
    {
        // ARRANGE — pregătim mock-urile pentru dependențe.
        var mockUtilizatorRepo = new Mock<IUtilizatorRepository>();
        var mockAutRepo = new Mock<IAutentificareRepository>();
        var mockHasher = new Mock<IPasswordHasher>();

        var utilizatorInBaza = new Administrator
        {
            Id = 1,
            Email = "admin@clinica.ro",
            ParolaHash = "hash-real-din-db",
            StatusCont = true,
            Rol = Rol.Admin,
            Nume = "Popescu",
            Prenume = "Ion",
            Telefon = "0700000000",
            Adresa = "Test 1"
        };

        // Repo-ul returnează utilizatorul, dar hasher-ul respinge parola.
        mockAutRepo.Setup(r => r.GetEsuateRecenteAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<Autentificare>().AsEnumerable());
        mockUtilizatorRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(utilizatorInBaza);
        mockHasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);  // parolă greșită

        var service = new AuthService(
            mockUtilizatorRepo.Object,
            mockAutRepo.Object,
            mockHasher.Object,
            NullLogger<AuthService>.Instance);

        // ACT
        var rezultat = await service.LoginAsync(
            "admin@clinica.ro", "parola-gresita", adresaIp: "127.0.0.1", userAgent: "test");

        // ASSERT
        Assert.False(rezultat.Succes, "Login cu parolă greșită nu trebuie să reușească.");
        Assert.NotNull(rezultat.Eroare);
        Assert.Contains("incorect", rezultat.Eroare!, StringComparison.OrdinalIgnoreCase);
    }
}
