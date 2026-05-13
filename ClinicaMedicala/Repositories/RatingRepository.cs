using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext _context;

    public RatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rating>> GetByMedicIdAsync(int medicId)
    {
        // Public: doar rating-urile pacient→medic, vizibile (nemoderate sau aprobate de admin)
        return await _context.Ratinguri
            .Where(r => r.MedicId == medicId && r.Vizibil && !r.AcordatDeMedic)
            .OrderByDescending(r => r.Data)
            .ToListAsync();
    }

    public async Task AddAsync(Rating rating)
    {
        _context.Ratinguri.Add(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<double> GetAverageRatingForMedicAsync(int medicId)
    {
        // Media: doar rating-urile pacient→medic, vizibile
        var ratings = await _context.Ratinguri
            .Where(r => r.MedicId == medicId && r.Vizibil && !r.AcordatDeMedic)
            .ToListAsync();

        return ratings.Any() ? ratings.Average(r => r.Scor) : 0;
    }
}
