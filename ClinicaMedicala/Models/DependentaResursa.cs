using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

// REQ-19: dependențe între resurse.
// Ex: "Aparat CT" (ResursaPrincipala) necesită "Sala CT" (ResursaCeruta).
// Când o programare include resursa principală, validatorul va impune și pe cea cerută.
[Index(nameof(ResursaPrincipalaId), nameof(ResursaCerutaId), IsUnique = true)]
public class DependentaResursa
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ResursaPrincipalaId { get; set; }

    [ForeignKey(nameof(ResursaPrincipalaId))]
    public Resursa ResursaPrincipala { get; set; } = null!;

    [Required]
    public int ResursaCerutaId { get; set; }

    [ForeignKey(nameof(ResursaCerutaId))]
    public Resursa ResursaCeruta { get; set; } = null!;

    [MaxLength(500)]
    public string? Descriere { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
