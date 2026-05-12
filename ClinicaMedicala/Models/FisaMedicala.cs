using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class FisaMedicala
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataCreare { get; set; } = DateTime.UtcNow;

    // Boli cronice diagnosticate (diabet, HTA, cardiopatie etc.)
    [MaxLength(2000)]
    public string? IstoricBoliCronice { get; set; }

    // Antecedente heredocolaterale — relevante clinic
    [MaxLength(1000)]
    public string? AntecedenteFamiliale { get; set; }

    // Grup de risc (ex: cardiovascular, oncologic) — util pentru screening
    [MaxLength(100)]
    public string? GrupaDeRisc { get; set; }

    // FK Pacient — one-to-one
    [Required]
    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;

    public ICollection<Consultatie> Consultatii { get; set; } = new List<Consultatie>();
}
