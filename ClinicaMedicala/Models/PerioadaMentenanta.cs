using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

// REQ-14: perioade programate de mentenanță pentru o resursă.
// Ex: „Cabinet RMN — în mentenanță 10-15 iunie 2026”.
// O resursă cu o perioadă activă (azi ∈ [Inceput, Sfarsit]) e indisponibilă pentru programări noi.
public class PerioadaMentenanta
{
    [Key]
    public int Id { get; set; }

    public int ResursaId { get; set; }

    [ForeignKey(nameof(ResursaId))]
    public Resursa Resursa { get; set; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Inceput { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Sfarsit { get; set; }

    [MaxLength(500)]
    public string? Descriere { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
