namespace ClinicaMedicala.Models;

public enum Rol
{
    Pacient,
    Medic,
    Asistent,
    Admin
}

public enum GradProfesional
{
    Specialist,
    Primar
}

public enum Tura
{
    Dimineata,
    Noapte
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
    AB_Negativ
}

public enum TipResursa
{
    Cabinet,
    Aparat_Imagistica,
    Analizor_Laborator
}

public enum StareResursa
{
    Functional,
    In_Mentenanta,
    Defect
}

public enum TipProgramare
{
    Consult_Initial,
    Control,
    Procedura
}

public enum StatusProgramare
{
    Programat,
    Confirmat,
    Finalizat,
    Anulat_Pacient,
    Anulat_Clinica
}

public enum TipDocument
{
    Analize_Sange,
    Imagistica,
    Bilet_Trimitere
}
