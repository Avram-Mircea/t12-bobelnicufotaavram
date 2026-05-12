namespace ClinicaMedicala.Services.Auth;

// Centralizare nume policies pentru a evita string-uri magice împrăștiate prin cod
public static class PoliciiAuth
{
    public const string DoarAdmin = "DoarAdmin";
    public const string DoarMedic = "DoarMedic";
    public const string DoarAsistent = "DoarAsistent";
    public const string DoarPacient = "DoarPacient";

    // Staff = toți utilizatorii interni (nu pacienții)
    public const string StaffClinica = "StaffClinica";
    public const string AdminSauMedic = "AdminSauMedic";
}
