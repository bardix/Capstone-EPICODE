using Capstone_EPICODE.Data;
using Capstone_EPICODE.Models.Parcheggio;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone_EPICODE.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ApplicationDbContext _context;

        public ParkingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parking>> GetAll()
        {
            return await _context.Parkings.ToListAsync();
        }

        public async Task<Parking> GetById(int id)
        {
            return await _context.Parkings.FindAsync(id);
        }

        public async Task<Parking> Create(Parking parking)
        {
            _context.Parkings.Add(parking);
            await _context.SaveChangesAsync();
            return parking;
        }

        public async Task<Parking> Update(Parking parking)
        {
            _context.Parkings.Update(parking);
            await _context.SaveChangesAsync();
            return parking;
        }

        public async Task<bool> Delete(int id)
        {
            var parking = await _context.Parkings.FindAsync(id);
            if (parking == null)
            {
                return false;
            }

            _context.Parkings.Remove(parking);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
