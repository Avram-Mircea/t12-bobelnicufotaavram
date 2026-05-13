using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class DocumentMedicalRepository : IDocumentMedicalRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentMedicalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentMedical>> GetByPacientIdAsync(int pacientId)
    {
        return await _context.DocumenteMedicale
            .Where(d => d.PacientId == pacientId)
            .OrderByDescending(d => d.DataIncarcare)
            .ToListAsync();
    }

    public async Task AddAsync(DocumentMedical doc)
    {
        _context.DocumenteMedicale.Add(doc);
        await _context.SaveChangesAsync();
    }

    public Task<DocumentMedical?> GetByIdAsync(int id) =>
        _context.DocumenteMedicale.FirstOrDefaultAsync(d => d.Id == id);

    public async Task UpdateAsync(DocumentMedical doc)
    {
        _context.DocumenteMedicale.Update(doc);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doc = await _context.DocumenteMedicale.FindAsync(id);
        if (doc != null)
        {
            _context.DocumenteMedicale.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }
}
