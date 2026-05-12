namespace ClinicaMedicala.Models;

public enum Rol
{
    Pacient,
    Medic,
    Asistent,
    Admin
}

// Valorile existente păstrate la pozițiile originale
// Valori noi adăugate la sfârșit pentru a nu rupe codul existent
public enum GradProfesional
{
    Specialist = 0,
    Primar = 1,
    Rezident = 2
}

public enum Tura
{
    Dimineata = 0,
    Noapte = 1,
    DupaAmiaza = 2
}

public enum GrupaSanguina
{
    O_Pozitiv,
    O_Negativ,
    A_Pozitiv,
    A_Negativ,
    B_Pozitiv,
    B_Negativ,
    AB_Pozitiv,
    AB_Negativ,
    Necunoscuta
}

public enum TipResursa
{
    Cabinet,
    Aparat_Imagistica,
    Analizor_Laborator,
    Sala_Proceduri,
    Sala_Operatii
}

public enum StareResursa
{
    Functional,
    In_Mentenanta,
    Defect,
    Rezervat
}

public enum TipProgramare
{
    Consult_Initial,
    Control,
    Procedura,
    Urgenta,
    Teleconsult
}

public enum StatusProgramare
{
    Programat,
    Confirmat,
    Finalizat,
    Anulat_Pacient,
    Anulat_Clinica,
    Neprezentare
}

// Valorile originale păstrate la aceleași poziții int
public enum TipDocument
{
    Analize_Sange = 0,
    Imagistica = 1,
    Bilet_Trimitere = 2,
    Analize_Urina = 3,
    Imagistica_Radiografie = 4,
    Imagistica_Ecografie = 5,
    Imagistica_CT = 6,
    Imagistica_RMN = 7,
    EKG = 8,
    Spirometrie = 9,
    Reteta = 10,
    Concediu_Medical = 11,
    Scrisoare_Medicala = 12,
    Rezultat_Biopsie = 13,
    Altele = 14
}

public enum CanalNotificare
{
    Email,
    SMS,
    Aplicatie
}
