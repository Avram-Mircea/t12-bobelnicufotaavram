using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

[Index(nameof(Email), IsUnique = true)]
public abstract class Utilizator
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nume { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Prenume { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    public string ParolaHash { get; set; } = null!;

    [Required]
    [MaxLength(15)]
    public string Telefon { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Adresa { get; set; } = null!;

    [Required]
    public Rol Rol { get; set; }

    public bool StatusCont { get; set; } = true;

    [Required]
    public DateTime DataCreareCont { get; set; } = DateTime.UtcNow;

    // Token securizat pentru resetare parolă (REQ-06)
    // Generat la cererea utilizatorului, șters după folosire sau expirare
    [MaxLength(128)]
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }
}
