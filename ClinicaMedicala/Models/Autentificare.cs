using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

// Jurnal de autentificări — obligatoriu în sistemele medicale (GDPR + HG 353/2022)
public class Autentificare
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataOra { get; set; } = DateTime.UtcNow;

    public bool Succes { get; set; }

    // IPv4 sau IPv6
    [MaxLength(45)]
    public string? AdresaIp { get; set; }

    // User-Agent pentru detectarea accesului suspect
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Required]
    public int UtilizatorId { get; set; }

    [ForeignKey(nameof(UtilizatorId))]
    public Utilizator Utilizator { get; set; } = null!;
}
