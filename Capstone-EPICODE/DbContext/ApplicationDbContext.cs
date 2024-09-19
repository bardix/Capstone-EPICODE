using Microsoft.EntityFrameworkCore;
using Capstone_EPICODE.Models;
using Capstone_EPICODE.Models.Parcheggio;
using Capstone_EPICODE.Models.Prenotazione;

namespace Capstone_EPICODE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Parking> Parkings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuriamo la relazione molti-a-molti tra Users e Roles

            modelBuilder.Entity<User>()
                .HasMany(p => p.Roles)
                .WithMany(i => i.Users)
                .UsingEntity(j => j.ToTable("RoleUsers")); // Tabella di giunzione
        }
    }
}
