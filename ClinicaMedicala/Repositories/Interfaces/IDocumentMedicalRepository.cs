using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories.Interfaces
{
    public interface IDocumentMedicalRepository
    {
        Task<List<DocumentMedical>> GetByPacientId(int pacientId);
        Task Add(DocumentMedical doc);
        Task<DocumentMedical?> GetById(int id);
        Task Update(DocumentMedical doc);
        Task Delete(int id);
    }
}
