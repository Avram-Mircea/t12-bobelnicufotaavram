using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models;

public class Asistent : Utilizator
{
    [Required]
    [MaxLength(100)]
    public string Departament { get; set; } = null!;

    [Required]
    public Tura Tura { get; set; }
}
