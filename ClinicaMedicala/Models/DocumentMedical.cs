using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class DocumentMedical
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TipDocument TipDocument { get; set; }

    [Required]
    [MaxLength(500)]
    public string CaleFisier { get; set; } = null!;

    [Required]
    public DateTime DataIncarcare { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Observatii { get; set; }

    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;
}
