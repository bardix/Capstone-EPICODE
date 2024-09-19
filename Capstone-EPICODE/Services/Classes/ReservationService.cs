using Capstone_EPICODE.Data;
using Capstone_EPICODE.Models.Prenotazione;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Capstone_EPICODE.Services.Classes
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> CreateReservation(int parkingId, int userId, DateTime start, DateTime end)
        {
            var parking = await _context.Parkings.FindAsync(parkingId);
            if (parking == null || !parking.IsAvailable)
                throw new Exception("Il parcheggio non è disponibile.");

            // Calcolo del prezzo in base al tempo
            var totalPrice = CalculatePrice(start, end);
            var reservation = new Reservation
            {
                ParkingId = parkingId,
                UserId = userId,
                ReservationStart = start,
                ReservationEnd = end,
                TotalPrice = totalPrice,
                IsActive = true
            };

            // Notifica al ParkingManager
            NotifyParkingManager(parking.ParkingManagerId, reservation);

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task CancelReservation(int reservationId)
        {
            var reservation = await GetReservationById(reservationId);
            if (reservation == null || !reservation.IsActive)
                throw new Exception("Prenotazione non trovata o già annullata.");

            reservation.IsActive = false;

            // Libera il parcheggio associato alla prenotazione
            var parking = await _context.Parkings.FindAsync(reservation.ParkingId);
            if (parking != null)
            {
                parking.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ExtendReservation(int reservationId, TimeSpan extensionDuration)
        {
            var reservation = await GetReservationById(reservationId);
            if (reservation == null || !reservation.IsActive)
                throw new Exception("Prenotazione non valida o già annullata.");

            // Estendi la fine della prenotazione in base alla durata
            reservation.ReservationEnd = reservation.ReservationEnd.Add(extensionDuration);

            // Aggiorna il prezzo in base alla durata estesa
            reservation.TotalPrice += CalculateExtensionPrice(extensionDuration);

            await _context.SaveChangesAsync();
        }

        private decimal CalculatePrice(DateTime start, DateTime end)
        {
            var hours = (end - start).TotalHours;
            return (decimal)(hours * 2); // Prezzo fisso di 2 euro all'ora
        }

        private decimal CalculateExtensionPrice(TimeSpan extensionDuration)
        {
            var quarters = (decimal)(extensionDuration.TotalMinutes / 15);  // Converti a decimal
            return quarters * 0.50m;  // 50 centesimi per quarto d'ora
        }

        private void NotifyParkingManager(int? parkingManagerId, Reservation reservation)
        {
            if (parkingManagerId.HasValue)
            {
                Console.WriteLine($"Notifica al ParkingManager con ID {parkingManagerId}: Prenotazione {reservation.Id} aggiornata.");
            }
        }

        public async Task<Reservation> GetReservationById(int id)
        {
            return await _context.Reservations.Include(r => r.Parking).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetUserReservations(int userId)
        {
            return await _context.Reservations.Include(r => r.Parking)
                                              .Where(r => r.UserId == userId && r.IsActive)
                                              .ToListAsync();
        }


        public async Task<IEnumerable<Reservation>> GetInactiveReservations()
        {
            return await _context.Reservations
                .Include(r => r.Parking)
                .Where(r => !r.IsActive)
                .ToListAsync();
        }

        public async Task DeleteReservation(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null || reservation.IsActive)
            {
                throw new Exception("Prenotazione non trovata o è ancora attiva.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
    }

}


