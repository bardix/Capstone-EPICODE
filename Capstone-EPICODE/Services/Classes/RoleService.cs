using Capstone_EPICODE.Data;
using Capstone_EPICODE.Models;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Capstone_EPICODE.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Role> Create(Role entity)
        {
            _context.Roles.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Role> Update(Role entity)
        {
            _context.Roles.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Role> Delete(int id)
        {
            // Trova il ruolo e include la relazione con gli utenti
            var role = await _context.Roles.Include(r => r.Users).FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                throw new Exception("Ruolo non trovato.");
            }

            // Verifica se il ruolo è associato a utenti
            if (role.Users.Any())  // Se ci sono utenti associati
            {
                throw new Exception("Il ruolo è associato a uno o più utenti e non può essere eliminato.");
            }

            // Elimina il ruolo se non è associato a utenti
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return role;
        }


        public async Task<Role> Read(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<IEnumerable<Role>> GetAll()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetByName(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        }

    }
}
