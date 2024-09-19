using Capstone_EPICODE.Models.Parcheggio;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_EPICODE.Models.Prenotazione
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ParkingId { get; set; }
        public Parking Parking { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime ReservationStart { get; set; }
        public DateTime ReservationEnd { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Specifica precisione e scala
        public decimal TotalPrice { get; set; }

        public bool IsActive { get; set; }
    }

}
