using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Resurse;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Services.Validare;

public class ConstraintValidationService : IConstraintValidationService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IReguliConsultatieService _reguli;

    public ConstraintValidationService(ApplicationDbContext ctx, IReguliConsultatieService reguli)
    {
        _ctx = ctx;
        _reguli = reguli;
    }

    public async Task<RezultatValidare> ValideazaProgramareAsync(CerereValidareProgramare cerere)
    {
        var rezultat = new RezultatValidare();

        // ── 1. Validare interval temporal de bază ─────────────────────────────
        if (cerere.DataEnd <= cerere.DataStart)
        {
            rezultat.AdaugaEroare("Data sfârșit trebuie să fie după data început.");
            return rezultat; // critical — abort
        }

        // ── 2. Medic activ și existent ────────────────────────────────────────
        var medic = await _ctx.Medici.FirstOrDefaultAsync(m => m.Id == cerere.MedicId);
        if (medic == null)
        {
            rezultat.AdaugaEroare("Medicul selectat nu există.");
            return rezultat;
        }
        if (!medic.StatusCont)
        {
            rezultat.AdaugaEroare($"Medicul „{medic.Prenume} {medic.Nume}” are contul dezactivat.");
        }

        // ── 3. Tip consultație → necesită asistent? (REQ-16) ──────────────────
        // La solicitare inițială de pacient (status Programat), nu cerem încă asistentul.
        // Asistenta îl va atașa la confirmare.
        var necesitaAsistent = await _reguli.NecesitaAsistentAsync(cerere.TipProgramare);

        if (necesitaAsistent && cerere.AsistentId == null && !cerere.EsteSolicitareDePacient)
        {
            rezultat.AdaugaEroare(
                $"Tipul „{cerere.TipProgramare.ToString().Replace('_', ' ')}” necesită prezența unui asistent.");
        }

        if (cerere.AsistentId.HasValue)
        {
            var asistent = await _ctx.Asistenti.FirstOrDefaultAsync(a => a.Id == cerere.AsistentId.Value);
            if (asistent == null)
                rezultat.AdaugaEroare("Asistentul selectat nu există.");
            else if (!asistent.StatusCont)
                rezultat.AdaugaEroare($"Asistentul „{asistent.Prenume} {asistent.Nume}” are contul dezactivat.");
        }

        // ── 4. Resurse: disponibilitate + specializare (REQ-12, REQ-15) ───────
        var resursaIds = cerere.ResursaIds.Distinct().ToList();
        if (resursaIds.Count > 0)
        {
            var resurse = await _ctx.Resurse
                .Include(r => r.Specializari)
                .Include(r => r.PerioadeMentenanta)
                .Include(r => r.DependenteIesite).ThenInclude(d => d.ResursaCeruta)
                .Where(r => resursaIds.Contains(r.Id))
                .ToListAsync();

            // 4a. Existență
            var gasiteIds = resurse.Select(r => r.Id).ToHashSet();
            foreach (var id in resursaIds.Where(id => !gasiteIds.Contains(id)))
                rezultat.AdaugaEroare($"Resursa cu ID {id} nu există.");

            foreach (var r in resurse)
            {
                ValideazaResursaDisponibilitate(r, cerere, rezultat);
                ValideazaResursaSpecializare(r, medic, rezultat);
            }

            // 4b. Dependențe (REQ-19): toate resursele cerute trebuie să fie în listă
            foreach (var r in resurse)
            {
                foreach (var dep in r.DependenteIesite)
                {
                    if (!gasiteIds.Contains(dep.ResursaCerutaId))
                    {
                        rezultat.AdaugaEroare(
                            $"Resursa „{r.Denumire}” depinde de „{dep.ResursaCeruta.Denumire}” " +
                            "care nu e inclusă în programare. Adaugă-o sau elimină dependența.");
                    }
                }
            }
        }

        return rezultat;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static void ValideazaResursaDisponibilitate(Resursa r, CerereValidareProgramare cerere, RezultatValidare rezultat)
    {
        if (!r.Activ)
        {
            rezultat.AdaugaEroare($"Resursa „{r.Denumire}” este inactivă administrativ.");
            return;
        }

        if (r.Stare != StareResursa.Functional && r.Stare != StareResursa.Rezervat)
        {
            rezultat.AdaugaEroare(
                $"Resursa „{r.Denumire}” are starea „{r.Stare.ToString().Replace('_', ' ')}” și nu poate fi folosită.");
        }

        if (r.DataScadentaRevizie.Date < cerere.DataStart.Date)
        {
            rezultat.AdaugaEroare(
                $"Resursa „{r.Denumire}” are revizia scadentă din {r.DataScadentaRevizie:dd.MM.yyyy}.");
        }

        // Mentenanță în interval
        var inMentenanta = r.PerioadeMentenanta.Any(p =>
            p.Inceput.Date <= cerere.DataEnd.Date &&
            p.Sfarsit.Date >= cerere.DataStart.Date);
        if (inMentenanta)
        {
            rezultat.AdaugaEroare(
                $"Resursa „{r.Denumire}” este programată în mentenanță în acel interval.");
        }
    }

    private static void ValideazaResursaSpecializare(Resursa r, Medic medic, RezultatValidare rezultat)
    {
        // REQ-15: dacă resursa are specializări permise, medicul trebuie să se încadreze.
        // Listă goală = resursa e universală (oricine o poate folosi).
        if (!r.Specializari.Any()) return;

        var match = r.Specializari.Any(s =>
            s.Nume.Equals(medic.Specializare, StringComparison.OrdinalIgnoreCase));

        if (!match)
        {
            var permise = string.Join(", ", r.Specializari.Select(s => s.Nume));
            rezultat.AdaugaEroare(
                $"Resursa „{r.Denumire}” e restricționată la specializările: {permise}. " +
                $"Medicul e specializat în „{medic.Specializare}”.");
        }
    }
}
