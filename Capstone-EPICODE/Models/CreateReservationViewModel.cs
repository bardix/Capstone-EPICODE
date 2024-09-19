using Capstone_EPICODE.Models.Parcheggio;

namespace Capstone_EPICODE.Models
{
    public class CreateReservationViewModel
    {
        public int ParkingId { get; set; }
        public int UserId { get; set; }  // Aggiungi l'ID dell'utente
        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }

        public IEnumerable<Parking>? AvailableParkings { get; set; }

        public int TotalPrice { get; set; }
    }
}
