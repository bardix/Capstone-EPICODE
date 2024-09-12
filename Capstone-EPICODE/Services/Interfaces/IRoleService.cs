using Capstone_EPICODE.Models;

namespace Capstone_EPICODE.Services.Interfaces
{
    public interface IRoleService
    {
        Task<Role> Create(Role entity);
        Task<Role> Update(Role entity);
        Task<Role> Delete(int id);
        Task<Role> Read(int id);
        Task<IEnumerable<Role>> GetAll();

        Task<Role> GetByName(string name);

    }
}
