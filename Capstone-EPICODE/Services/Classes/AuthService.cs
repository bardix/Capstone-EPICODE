using Capstone_EPICODE.Data;
using Capstone_EPICODE.Models;
using Capstone_EPICODE.Services.Interfaces;
using Capstone_EPICODE.Services.Password;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Capstone_EPICODE.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordEnconder _passwordEncoder;
        private readonly ApplicationDbContext _db;

        public AuthService(IPasswordEnconder passwordEncoder, ApplicationDbContext db)
        {
            _passwordEncoder = passwordEncoder;
            _db = db;
        }

        // Aggiunta di un ruolo all'utente
        public async Task<User> AddRoleToUser(int userId, string roleName)
        {
            var user = await _db.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
            var role = await _db.Roles.SingleOrDefaultAsync(r => r.Name == roleName);

            if (user == null || role == null)
            {
                return null!;
            }

            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);
                await _db.SaveChangesAsync();
            }

            return user;
        }

        // Creazione di un nuovo utente con ruoli selezionati
        public async Task<User> Create(UserViewModel entity, IEnumerable<int> roleSelected)
        {
            var user = new User
            {
                Name = entity.Name,
                Email = entity.Email,
                Password = _passwordEncoder.Encode(entity.Password),  // Usa l'encoder personalizzato
            };

            var roles = await _db.Roles.Where(r => roleSelected.Contains(r.Id)).ToListAsync();
            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        // Eliminazione di un utente
        public async Task<User> Delete(int id)
        {
            var user = await GetById(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
            return user;
        }

        // Recupero di tutti gli utenti
        public async Task<IEnumerable<User>> GetAll()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .ToListAsync();

            return users;
        }

        // Recupero di un utente per ID
        public async Task<User> GetById(int id)
        {
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(x => x.Id == id);

            return user ?? null!;
        }

        // Login
        public async Task<User> Login(UserViewModel entity)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == entity.Email);
            if (user != null && _passwordEncoder.IsSame(entity.Password, user.Password))  // Confronto password
            {
                var userResulted = new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Roles = user.Roles.ToList(),
                    Id = user.Id,
                };
                return userResulted;
            }
            return null!;
        }

        // Rimozione di un ruolo da un utente
        public async Task<User> RemoveRoleToUser(int userId, string roleName)
        {
            var user = await _db.Users.Include(u => u.Roles).SingleOrDefaultAsync(u => u.Id == userId);
            var role = await _db.Roles.SingleOrDefaultAsync(r => r.Name == roleName);

            if (user == null || role == null)
            {
                return null!;
            }

            if (user.Roles.Contains(role))
            {
                user.Roles.Remove(role);
                await _db.SaveChangesAsync();
            }

            return user;
        }
    }
}
