using Capstone_EPICODE.Models;
using Capstone_EPICODE.Models.Prenotazione;
using Capstone_EPICODE.Services;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Capstone_EPICODE.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IParkingService _parkingService;
        private readonly IAuthService _authService;// Aggiungi IParkingService

        public ReservationController(IReservationService reservationService, IParkingService parkingService ,IAuthService authService) // Aggiungi IParkingService al costruttore
        {
            _reservationService = reservationService;
            _parkingService = parkingService;
            _authService = authService;// Inizializza IParkingService
        }


        // Aggiungiamo l'azione per mostrare le informazioni utente e le prenotazioni
        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            // Recupera l'ID dell'utente autenticato
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Recupera i dettagli dell'utente
            var user = await _authService.GetById(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            // Recupera tutte le prenotazioni dell'utente
            var reservations = await _reservationService.GetUserReservations(int.Parse(userId));

            // Crea un ViewModel per passare i dati dell'utente e le prenotazioni alla vista
            var userProfileViewModel = new UserProfileViewModel
            {
                User = user,
                Reservations = reservations
            };

            return View(userProfileViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            await _reservationService.CancelReservation(reservationId);
            return RedirectToAction("MyReservation");
        }

        [HttpPost]
        public async Task<IActionResult> Extend(int reservationId, TimeSpan extensionDuration)
        {
            await _reservationService.ExtendReservation(reservationId, extensionDuration);
            return RedirectToAction("MyReservations");
        }

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            // Attendi il completamento del Task per ottenere la lista di parcheggi
            var availableParkings = (await _parkingService.GetAll())
                .Where(p => p.IsAvailable)
                .ToList(); // Applica il filtro dopo aver atteso il Task

            var reservationViewModel = new CreateReservationViewModel
            {
                AvailableParkings = availableParkings,  // Aggiungi la lista dei parcheggi disponibili
                ReservationStart = DateTime.Now,
                ReservationEnd = DateTime.Now.AddHours(1)
            };

            return View(reservationViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(CreateReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Crea la prenotazione nel servizio
                    var reservation = await _reservationService.CreateReservation(
                        model.ParkingId,
                        int.Parse(userId),
                        model.ReservationStart,
                        model.ReservationEnd
                    );

                    // Imposta il prezzo calcolato nella vista del modello
                    model.TotalPrice = (int)reservation.TotalPrice;

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Ripopola i parcheggi disponibili in caso di errore di validazione
            var availableParkings = (await _parkingService.GetAll())
                .Where(p => p.IsAvailable)
                .ToList();  // Applica il filtro dopo aver atteso il completamento del Task

            model.AvailableParkings = availableParkings;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(CreateReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Logica per confermare la prenotazione
                await _reservationService.CreateReservation(model.ParkingId, model.UserId, model.ReservationStart, model.ReservationEnd);

                return RedirectToAction("ConfirmReservation");
            }

            return View(model); 
        }


    }
}
