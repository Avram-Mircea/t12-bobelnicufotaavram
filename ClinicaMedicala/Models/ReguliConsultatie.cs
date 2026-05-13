using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

// REQ-16: definirea regulilor per tip de consultație.
// Există exact o regulă per TipProgramare (unicitate impusă la nivel DB).
[Index(nameof(TipProgramare), IsUnique = true)]
public class ReguliConsultatie
{
    [Key]
    public int Id { get; set; }

    [Required]
    public TipProgramare TipProgramare { get; set; }

    // True → la creare programare de acest tip, e obligatoriu un asistent atașat
    public bool NecesitaAsistent { get; set; }

    [MaxLength(500)]
    public string? Descriere { get; set; }
}
