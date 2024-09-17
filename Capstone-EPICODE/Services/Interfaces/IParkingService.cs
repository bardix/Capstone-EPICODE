using Capstone_EPICODE.Models.Parcheggio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone_EPICODE.Services.Interfaces
{
    public interface IParkingService
    {
        Task<IEnumerable<Parking>> GetAll();
        Task<Parking> GetById(int id);
        Task<Parking> Create(Parking parking);
        Task<Parking> Update(Parking parking);
        Task<bool> Delete(int id);
    }
}