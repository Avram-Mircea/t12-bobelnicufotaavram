namespace ClinicaMedicala.Services.Validare;

// REQ-17: validare automată constrângeri la creare programare.
// REQ-18: blocare salvare programare dacă restricții încălcate.
// Consumat de Management Programări (colegul cu Programări) înainte de SaveChanges.
public interface IConstraintValidationService
{
    Task<RezultatValidare> ValideazaProgramareAsync(CerereValidareProgramare cerere);
}
