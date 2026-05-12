using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

[Index(nameof(Nume), IsUnique = true)]
public class Specializare
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nume { get; set; } = null!;

    [MaxLength(500)]
    public string? Descriere { get; set; }

    // Soft delete — adminul poate dezactiva fără să șteargă (afectează doar dropdown-urile noi)
    public bool Activ { get; set; } = true;

    // Many-to-many cu Resursa: o resursă poate fi alocată pentru mai multe specializări
    // și o specializare poate folosi mai multe resurse.
    public ICollection<Resursa> Resurse { get; set; } = new List<Resursa>();
}
