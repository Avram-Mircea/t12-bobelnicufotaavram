using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Models;


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

    
    [Required]
    public bool Activ { get; set; } = true;

 
    [Required]
    [MaxLength(50)]
    public string NumarInventar { get; set; } = null!;

    
    [MaxLength(100)]
    public string? Locatie { get; set; }

 
    public ICollection<Specializare> Specializari { get; set; } = new List<Specializare>();


    [Required]
    public DateTime DataUltimaRevizie { get; set; }

    [Required]
    public DateTime DataScadentaRevizie { get; set; }


    public int AdministratorId { get; set; }

    [ForeignKey(nameof(AdministratorId))]
    public Administrator Administrator { get; set; } = null!;

    public ICollection<Programare> Programari { get; set; } = new List<Programare>();


    public ICollection<PerioadaMentenanta> PerioadeMentenanta { get; set; } = new List<PerioadaMentenanta>();

    
    public ICollection<DependentaResursa> DependenteIesite { get; set; } = new List<DependentaResursa>();

    public ICollection<DependentaResursa> DependenteIntrate { get; set; } = new List<DependentaResursa>();
}
