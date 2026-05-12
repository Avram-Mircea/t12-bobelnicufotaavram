using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ── DbSets ────────────────────────────────────────────────────────────────
    public DbSet<Utilizator> Utilizatori { get; set; } = null!;
    public DbSet<Medic> Medici { get; set; } = null!;
    public DbSet<Asistent> Asistenti { get; set; } = null!;
    public DbSet<Pacient> Pacienti { get; set; } = null!;
    public DbSet<Administrator> Administratori { get; set; } = null!;
    public DbSet<Autentificare> Autentificari { get; set; } = null!;
    public DbSet<Resursa> Resurse { get; set; } = null!;
    public DbSet<Programare> Programari { get; set; } = null!;
    public DbSet<FisaMedicala> FiseMedicale { get; set; } = null!;
    public DbSet<Consultatie> Consultatii { get; set; } = null!;
    public DbSet<DocumentMedical> DocumenteMedicale { get; set; } = null!;
    public DbSet<Rating> Ratinguri { get; set; } = null!;
    public DbSet<Specializare> Specializari { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── TPT: fiecare tip de utilizator are propria tabelă ─────────────────
        // Utilizatori → date comune
        // Medici / Pacienti / Asistenti / Administratori → date specifice tipului
        modelBuilder.Entity<Utilizator>().UseTptMappingStrategy();

        // ── MEDIC ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Medic>()
            .HasIndex(m => m.CodParafa)
            .IsUnique();

        modelBuilder.Entity<Medic>()
            .Property(m => m.CostConsultatie)
            .HasColumnType("decimal(18,2)");

        // ── RESURSA ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Resursa>()
            .HasOne(r => r.Administrator)
            .WithMany(a => a.ResurseAdministrate)
            .HasForeignKey(r => r.AdministratorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── PROGRAMARE ────────────────────────────────────────────────────────
        modelBuilder.Entity<Programare>()
            .HasOne(p => p.Pacient)
            .WithMany(pa => pa.Programari)
            .HasForeignKey(p => p.PacientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Programare>()
            .HasOne(p => p.Medic)
            .WithMany(m => m.Programari)
            .HasForeignKey(p => p.MedicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Programare>()
            .HasOne(p => p.Asistent)
            .WithMany(a => a.ProgramariGestionate)
            .HasForeignKey(p => p.AsistentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Programare>()
            .HasOne(p => p.Resursa)
            .WithMany(r => r.Programari)
            .HasForeignKey(p => p.ResursaId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── FISA MEDICALA: one-to-one cu Pacient ──────────────────────────────
        modelBuilder.Entity<FisaMedicala>()
            .HasOne(f => f.Pacient)
            .WithOne(p => p.FisaMedicala)
            .HasForeignKey<FisaMedicala>(f => f.PacientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── CONSULTATIE ───────────────────────────────────────────────────────
        modelBuilder.Entity<Consultatie>()
            .HasOne(c => c.Medic)
            .WithMany(m => m.Consultatii)
            .HasForeignKey(c => c.MedicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Consultatie>()
            .HasOne(c => c.FisaMedicala)
            .WithMany(f => f.Consultatii)
            .HasForeignKey(c => c.FisaMedicalaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Legătură opțională one-to-one consultație ↔ programare
        modelBuilder.Entity<Consultatie>()
            .HasOne(c => c.Programare)
            .WithOne(p => p.Consultatie)
            .HasForeignKey<Consultatie>(c => c.ProgramareId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── DOCUMENT MEDICAL ──────────────────────────────────────────────────
        modelBuilder.Entity<DocumentMedical>()
            .HasOne(d => d.Pacient)
            .WithMany(p => p.DocumenteMedicale)
            .HasForeignKey(d => d.PacientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentMedical>()
            .HasOne(d => d.Medic)
            .WithMany(m => m.DocumenteIncarcate)
            .HasForeignKey(d => d.MedicId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── RATING ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Pacient)
            .WithMany(p => p.Ratinguri)
            .HasForeignKey(r => r.PacientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Medic)
            .WithMany(m => m.Ratinguri)
            .HasForeignKey(r => r.MedicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un pacient poate acorda un singur rating unui medic
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.PacientId, r.MedicId })
            .IsUnique();

        // ── AUTENTIFICARE ─────────────────────────────────────────────────────
        modelBuilder.Entity<Autentificare>()
            .HasOne(a => a.Utilizator)
            .WithMany()
            .HasForeignKey(a => a.UtilizatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── MEDIC ↔ PACIENT: many-to-many ─────────────────────────────────────
        // Cu TPT, fiecare derived table are propria sa cascadă spre Utilizatori,
        // deci junction table-urile trebuie să aibă Restrict pe ambele FKs
        // pentru a evita multiple cascade paths detectate de SQL Server.
        modelBuilder.Entity<Medic>()
            .HasMany(m => m.Pacienti)
            .WithMany(p => p.Medici)
            .UsingEntity(
                "MedicPacienti",
                j => j.HasOne(typeof(Pacient)).WithMany()
                      .HasForeignKey("PacientiId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne(typeof(Medic)).WithMany()
                      .HasForeignKey("MediciId").OnDelete(DeleteBehavior.Restrict)
            );

        // ── MEDIC ↔ ASISTENT: many-to-many ────────────────────────────────────
        modelBuilder.Entity<Medic>()
            .HasMany(m => m.Asistenti)
            .WithMany(a => a.Medici)
            .UsingEntity(
                "MedicAsistenti",
                j => j.HasOne(typeof(Asistent)).WithMany()
                      .HasForeignKey("AsistentiId").OnDelete(DeleteBehavior.Restrict),
                j => j.HasOne(typeof(Medic)).WithMany()
                      .HasForeignKey("MediciId").OnDelete(DeleteBehavior.Restrict)
            );

        // ── RESURSA ↔ SPECIALIZARE: many-to-many (REQ-13) ─────────────────────
        modelBuilder.Entity<Resursa>()
            .HasMany(r => r.Specializari)
            .WithMany(s => s.Resurse)
            .UsingEntity(j => j.ToTable("ResursaSpecializare"));

        // ── Seed specializări medicale uzuale (Ord. MS 1509/2008) ─────────────
        modelBuilder.Entity<Specializare>().HasData(
            new Specializare { Id = 1,  Nume = "Medicină de familie",     Activ = true },
            new Specializare { Id = 2,  Nume = "Medicină internă",         Activ = true },
            new Specializare { Id = 3,  Nume = "Cardiologie",              Activ = true },
            new Specializare { Id = 4,  Nume = "Pediatrie",                Activ = true },
            new Specializare { Id = 5,  Nume = "Chirurgie generală",       Activ = true },
            new Specializare { Id = 6,  Nume = "Ortopedie și traumatologie", Activ = true },
            new Specializare { Id = 7,  Nume = "Obstetrică-Ginecologie",   Activ = true },
            new Specializare { Id = 8,  Nume = "Neurologie",               Activ = true },
            new Specializare { Id = 9,  Nume = "Dermatologie",             Activ = true },
            new Specializare { Id = 10, Nume = "Oftalmologie",             Activ = true },
            new Specializare { Id = 11, Nume = "ORL",                       Activ = true },
            new Specializare { Id = 12, Nume = "Stomatologie",             Activ = true },
            new Specializare { Id = 13, Nume = "Endocrinologie",           Activ = true },
            new Specializare { Id = 14, Nume = "Psihiatrie",               Activ = true },
            new Specializare { Id = 15, Nume = "Radiologie imagistică",    Activ = true }
        );
    }
}
