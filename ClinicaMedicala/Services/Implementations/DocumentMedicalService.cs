using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories.Interfaces;
using ClinicaMedicala.Services.Interfaces;

namespace ClinicaMedicala.Services.Implementations
{
    public class DocumentMedicalService : IDocumentMedicalService
    {
        private readonly IDocumentMedicalRepository _repo;

        public DocumentMedicalService(IDocumentMedicalRepository repo)
        {
            _repo = repo;
        }

        public Task<List<DocumentMedical>> GetByPacientId(int pacientId)
            => _repo.GetByPacientId(pacientId);

        public async Task Add(DocumentMedical document)
        {
            await _repo.Add(document);
        }

        public Task<DocumentMedical?> GetById(int id)
    => _repo.GetById(id);

        public Task Update(DocumentMedical doc)
            => _repo.Update(doc);

        public Task Delete(int id)
            => _repo.Delete(id);
    }
}
