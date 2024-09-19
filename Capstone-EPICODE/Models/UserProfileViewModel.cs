using Capstone_EPICODE.Models.Prenotazione;


namespace Capstone_EPICODE.Models
{
    public class UserProfileViewModel
    {
        public User User { get; set; }
        public IEnumerable<Reservation> Reservations { get; set; }
    }
}