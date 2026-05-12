using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;

// REQ-09: Denumirea trebuie să fie unică în sistem (alături de NumarInventar)
[Index(nameof(NumarInventar), IsUnique = true)]
[Index(nameof(Denumire), IsUnique = true)]
public class Resursa
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Denumire { get; set; } = null!;

    [Required]
    public TipResursa Tip { get; set; }

    [Required]
    public StareResursa Stare { get; set; }

    // Număr de inventar — obligatoriu în unitățile medicale din România (Ord. 2861/2009)
    [Required]
    [MaxLength(50)]
    public string NumarInventar { get; set; } = null!;

    // Localizare fizică (ex: "Etaj 2, Camera 204")
    [MaxLength(100)]
    public string? Locatie { get; set; }

    // REQ-13: specializările medicale care pot utiliza această resursă.
    // Many-to-many — o sală poate fi folosită de Cardiologie + Pediatrie etc.
    public ICollection<Specializare> Specializari { get; set; } = new List<Specializare>();

    // Dată revizie tehnică — aparatele medicale au revizie periodică obligatorie
    [Required]
    public DateTime DataUltimaRevizie { get; set; }

    [Required]
    public DateTime DataScadentaRevizie { get; set; }

    // FK Administrator
    public int AdministratorId { get; set; }

    [ForeignKey(nameof(AdministratorId))]
    public Administrator Administrator { get; set; } = null!;

    public ICollection<Programare> Programari { get; set; } = new List<Programare>();
}
