using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

[Table("Administratori")]
public class Administrator : Utilizator
{
    public ICollection<Resursa> ResurseAdministrate { get; set; } = new List<Resursa>();
}
