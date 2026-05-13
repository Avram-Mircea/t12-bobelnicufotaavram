using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Pacienti;

public interface IDocumentMedicalService
{
    Task<List<DocumentMedical>> GetByPacientIdAsync(int pacientId);
    Task AddAsync(DocumentMedical document);
    Task<DocumentMedical?> GetByIdAsync(int id);
    Task UpdateAsync(DocumentMedical doc);
    Task DeleteAsync(int id);
}
