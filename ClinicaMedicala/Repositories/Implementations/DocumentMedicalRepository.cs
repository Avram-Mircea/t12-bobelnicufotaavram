using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories.Implementations
{
    public class DocumentMedicalRepository : IDocumentMedicalRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentMedicalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentMedical>> GetByPacientId(int pacientId)
        {
            return await _context.DocumenteMedicale
                .Where(d => d.PacientId == pacientId)
                .OrderByDescending(d => d.DataIncarcare)
                .ToListAsync();
        }

        public async Task Add(DocumentMedical doc)
        {
            _context.DocumenteMedicale.Add(doc);
            await _context.SaveChangesAsync();
        }
        public async Task<DocumentMedical?> GetById(int id)
        {
            return await _context.DocumenteMedicale
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task Update(DocumentMedical doc)
        {
            _context.DocumenteMedicale.Update(doc);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var doc = await _context.DocumenteMedicale.FindAsync(id);

            if (doc != null)
            {
                _context.DocumenteMedicale.Remove(doc);
                await _context.SaveChangesAsync();
            }
        }
    }
}
