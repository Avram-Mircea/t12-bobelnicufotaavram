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

    
    public bool Activ { get; set; } = true;

    public ICollection<Resursa> Resurse { get; set; } = new List<Resursa>();
}
