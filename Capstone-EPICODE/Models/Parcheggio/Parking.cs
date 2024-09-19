namespace Capstone_EPICODE.Models.Parcheggio
{
    public class Parking
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Relazione con il ParkingManager
        public int? ParkingManagerId { get; set; }
        public User? ParkingManager { get; set; }

        public int? ActiveReservationCount { get; set; } = 0; // Contatore inizializzato a 0
    }
}
