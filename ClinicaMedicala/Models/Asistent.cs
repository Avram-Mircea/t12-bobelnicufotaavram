using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

[Table("Asistenti")]
public class Asistent : Utilizator
{
    [Required]
    [MaxLength(100)]
    public string Departament { get; set; } = null!;

    [Required]
    public Tura Tura { get; set; }

    // Many-to-many: medicii cu care lucrează acest asistent
    public ICollection<Medic> Medici { get; set; } = new List<Medic>();

    // Programările gestionate/confirmate de acest asistent
    public ICollection<Programare> ProgramariGestionate { get; set; } = new List<Programare>();
}
