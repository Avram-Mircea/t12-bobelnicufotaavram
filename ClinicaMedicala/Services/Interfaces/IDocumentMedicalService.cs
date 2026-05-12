using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Interfaces
{
    public interface IDocumentMedicalService
    {
        Task<List<DocumentMedical>> GetByPacientId(int pacientId);
        Task Add(DocumentMedical document);
        Task<DocumentMedical?> GetById(int id);
        Task Update(DocumentMedical doc);
        Task Delete(int id);
    }
}
