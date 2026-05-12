using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class ReguliConsultatieViewModel
{
    public List<ReguliConsultatieRow> Reguli { get; set; } = new();

    public class ReguliConsultatieRow
    {
        public int Id { get; set; }
        public TipProgramare TipProgramare { get; set; }
        public bool NecesitaAsistent { get; set; }
        [MaxLength(500)]
        public string? Descriere { get; set; }
    }
}
