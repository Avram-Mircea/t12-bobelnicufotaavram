using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models;

public class Medic : Utilizator
{
    [Required]
    [MaxLength(100)]
    public string Specializare { get; set; } = null!;

    [Required]
    [MaxLength(6)]
    public string CodParafa { get; set; } = null!;

    [Required]
    public GradProfesional GradProfesional { get; set; }

    [Required]
    public decimal CostConsultatie { get; set; }

    public ICollection<Programare> Programari { get; set; } = new List<Programare>();
    public ICollection<Consultatie> Consultatii { get; set; } = new List<Consultatie>();
}
