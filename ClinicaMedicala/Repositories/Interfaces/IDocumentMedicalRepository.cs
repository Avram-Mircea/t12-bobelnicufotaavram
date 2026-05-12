using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories.Interfaces
{
    public interface IDocumentMedicalRepository
    {
        Task<List<DocumentMedical>> GetByPacientId(int pacientId);
        Task Add(DocumentMedical doc);
    }
}
