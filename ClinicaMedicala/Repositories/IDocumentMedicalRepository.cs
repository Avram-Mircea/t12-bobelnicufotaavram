using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IDocumentMedicalRepository
{
    Task<List<DocumentMedical>> GetByPacientIdAsync(int pacientId);
    Task AddAsync(DocumentMedical doc);
    Task<DocumentMedical?> GetByIdAsync(int id);
    Task UpdateAsync(DocumentMedical doc);
    Task DeleteAsync(int id);
}
