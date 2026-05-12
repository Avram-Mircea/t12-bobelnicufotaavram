using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class Rating
{
    [Key]
    public int Id { get; set; }

    // Scor de la 1 la 5 stele
    [Required]
    [Range(1, 5)]
    public int Scor { get; set; }

    [MaxLength(1000)]
    public string? Comentariu { get; set; }

    [Required]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    // Setat true de admin după moderare — din diagrama de cazuri de utilizare
    public bool Moderat { get; set; } = false;

    // Adminul poate ascunde comentarii inadecvate fără a le șterge
    public bool Vizibil { get; set; } = true;

    [Required]
    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;

    [Required]
    public int MedicId { get; set; }

    [ForeignKey(nameof(MedicId))]
    public Medic Medic { get; set; } = null!;
}
