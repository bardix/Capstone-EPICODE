using Capstone_EPICODE.Models.Prenotazione;

namespace Capstone_EPICODE.Services.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservation(int parkingId, int userId, DateTime start, DateTime end);
        Task<Reservation> GetReservationById(int id);
        Task<IEnumerable<Reservation>> GetUserReservations(int userId);
        Task CancelReservation(int reservationId);
        Task ExtendReservation(int reservationId, TimeSpan extensionDuration);
    }

}
