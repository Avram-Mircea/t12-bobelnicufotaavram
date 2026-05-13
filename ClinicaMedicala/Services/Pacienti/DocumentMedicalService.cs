using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Pacienti;

public class DocumentMedicalService : IDocumentMedicalService
{
    private readonly IDocumentMedicalRepository _repo;

    public DocumentMedicalService(IDocumentMedicalRepository repo)
    {
        _repo = repo;
    }

    public Task<List<DocumentMedical>> GetByPacientIdAsync(int pacientId) =>
        _repo.GetByPacientIdAsync(pacientId);

    public Task AddAsync(DocumentMedical document) => _repo.AddAsync(document);
    public Task<DocumentMedical?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
    public Task UpdateAsync(DocumentMedical doc) => _repo.UpdateAsync(doc);
    public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
}
