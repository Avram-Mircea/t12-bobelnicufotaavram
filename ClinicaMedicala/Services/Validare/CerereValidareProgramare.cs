using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Validare;

// Input pentru validatorul de programare (REQ-17, REQ-18).
// Construit de colegul cu Programări înainte de a salva o programare nouă.
public class CerereValidareProgramare
{
    public int MedicId { get; set; }
    public TipProgramare TipProgramare { get; set; }
    public int? AsistentId { get; set; }
    public List<int> ResursaIds { get; set; } = new();
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }

    // True = solicitare inițială de pacient (status Programat, urmează confirmare staff).
    // Validatorul nu cere prezența asistentei — asistenta o va atașa la confirmare.
    public bool EsteSolicitareDePacient { get; set; } = false;
}
