using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

[Table("Pacienti")]
[Index(nameof(CNP), IsUnique = true)]
public class Pacient : Utilizator
{
    // Cod numeric personal — 13 cifre, identificator unic în România
    [Required]
    [StringLength(13, MinimumLength = 13)]
    public string CNP { get; set; } = null!;

    [Required]
    public DateTime DataNastere { get; set; }

    // Asigurat CNAS — relevant pentru decontarea serviciilor medicale
    public bool AsiguratCNAS { get; set; }

    [Required]
    public GrupaSanguina GrupaSanguina { get; set; }

    [MaxLength(500)]
    public string? AlergiiCunoscute { get; set; }

    // Contact de urgență — obligatoriu în practica clinică
    [Required]
    [MaxLength(150)]
    public string ContactUrgentaNume { get; set; } = null!;

    [Required]
    [MaxLength(15)]
    public string ContactUrgentaTelefon { get; set; } = null!;

    // Navigări
    public FisaMedicala? FisaMedicala { get; set; }
    public ICollection<Programare> Programari { get; set; } = new List<Programare>();
    public ICollection<DocumentMedical> DocumenteMedicale { get; set; } = new List<DocumentMedical>();
    public ICollection<Rating> Ratinguri { get; set; } = new List<Rating>();

    // Many-to-many: medicii curanți ai pacientului
    public ICollection<Medic> Medici { get; set; } = new List<Medic>();
}
