using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories.Implementations
{
    public class RatingRepository : IRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public RatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rating>> GetByMedicId(int medicId)
        {
            return await _context.Ratinguri
                .Where(r => r.MedicId == medicId && r.Vizibil)
                .OrderByDescending(r => r.Data)
                .ToListAsync();
        }

        public async Task Add(Rating rating)
        {
            _context.Ratinguri.Add(rating);
            await _context.SaveChangesAsync();
        }
    }
}
