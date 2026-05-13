using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class DocumentMedical
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TipDocument TipDocument { get; set; }

    // Cale relativă sau URL în storage (ex: /documente/2026/reteta_123.pdf)
    [Required]
    [MaxLength(500)]
    public string CaleFisier { get; set; } = null!;

    [Required]
    public DateTime DataIncarcare { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Observatii { get; set; }

    // FK Pacient
    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;

    // FK Medic — cel care a emis/încărcat documentul (opțional: poate fi și asistent)
    public int? MedicId { get; set; }

    [ForeignKey(nameof(MedicId))]
    public Medic? Medic { get; set; }
}
