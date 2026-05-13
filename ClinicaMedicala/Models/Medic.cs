using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

[Table("Medici")]
public class Medic : Utilizator
{
    [Required]
    [MaxLength(100)]
    public string Specializare { get; set; } = null!;

    // Cod de parafă — 6 caractere, obligatoriu pentru prescripții în România
    [Required]
    [MaxLength(6)]
    public string CodParafa { get; set; } = null!;

    [Required]
    public GradProfesional GradProfesional { get; set; }

    // Tarif consultație pentru clinici private
    [Required]
    public decimal CostConsultatie { get; set; }

    // Număr contract cu Casa de Asigurări de Sănătate (opțional — clinici private pot fi necontractate)
    [MaxLength(50)]
    public string? NumarContractCAS { get; set; }

    // Navigări
    public ICollection<Programare> Programari { get; set; } = new List<Programare>();
    public ICollection<Consultatie> Consultatii { get; set; } = new List<Consultatie>();
    public ICollection<DocumentMedical> DocumenteIncarcate { get; set; } = new List<DocumentMedical>();
    public ICollection<Rating> Ratinguri { get; set; } = new List<Rating>();

    // Many-to-many: medicii curanți ai unui pacient
    public ICollection<Pacient> Pacienti { get; set; } = new List<Pacient>();

    // Many-to-many: asistentii care lucrează cu acest medic
    public ICollection<Asistent> Asistenti { get; set; } = new List<Asistent>();
}
