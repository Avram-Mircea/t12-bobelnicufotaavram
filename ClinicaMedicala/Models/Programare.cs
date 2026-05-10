using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class Programare
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataStart { get; set; }

    [Required]
    public DateTime DataEnd { get; set; }

    [Required]
    [MaxLength(500)]
    public string MotivVizita { get; set; } = null!;

    [Required]
    public TipProgramare TipProgramare { get; set; }

    [Required]
    public StatusProgramare Status { get; set; }

    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;

    public int MedicId { get; set; }

    [ForeignKey(nameof(MedicId))]
    public Medic Medic { get; set; } = null!;

    public int? ResursaId { get; set; }

    [ForeignKey(nameof(ResursaId))]
    public Resursa? Resursa { get; set; }
}
