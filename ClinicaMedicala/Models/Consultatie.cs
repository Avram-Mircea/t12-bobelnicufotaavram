using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class Consultatie
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Data { get; set; }

    [MaxLength(1000)]
    public string? SimptomePrezentate { get; set; }

    // Cod diagnostic conform clasificării internaționale ICD-10
    [MaxLength(20)]
    public string? DiagnosticICD10 { get; set; }

    [MaxLength(1000)]
    public string? TratamentRecomandat { get; set; }

    [MaxLength(1000)]
    public string? ObservatiiMedic { get; set; }

    // FK FisaMedicala
    public int FisaMedicalaId { get; set; }

    [ForeignKey(nameof(FisaMedicalaId))]
    public FisaMedicala FisaMedicala { get; set; } = null!;

    // FK Medic
    public int MedicId { get; set; }

    [ForeignKey(nameof(MedicId))]
    public Medic Medic { get; set; } = null!;

    // FK Programare — legătură opțională (consultațiile de urgență pot fi fără programare)
    public int? ProgramareId { get; set; }

    [ForeignKey(nameof(ProgramareId))]
    public Programare? Programare { get; set; }
}
