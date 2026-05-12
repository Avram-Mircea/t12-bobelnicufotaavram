namespace ClinicaMedicala.Services.Validare;

// Rezultatul validării unei programări — listă acumulată de erori.
// EValida = true ⟺ nicio eroare găsită (REQ-18).
public class RezultatValidare
{
    public List<string> Erori { get; } = new();

    public bool EValida => Erori.Count == 0;

    public void AdaugaEroare(string mesaj)
    {
        if (!string.IsNullOrWhiteSpace(mesaj))
            Erori.Add(mesaj);
    }
}
