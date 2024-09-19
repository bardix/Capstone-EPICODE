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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int reservationId)
        {
            try
            {
                // Elimina la prenotazione con IsActive impostato su false
                await _reservationService.DeleteReservation(reservationId);
                TempData["SuccessMessage"] = "Prenotazione eliminata con successo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Errore durante l'eliminazione della prenotazione: {ex.Message}";
            }

            return RedirectToAction("InactiveReservations");
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            try
            {
                await _reservationService.CancelReservation(reservationId);
                TempData["SuccessMessage"] = "La prenotazione è stata annullata con successo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Errore durante l'annullamento della prenotazione: {ex.Message}";
            }

            return RedirectToAction("UserProfile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int reservationId, int extensionDurationInMinutes)
        {
            if (extensionDurationInMinutes <= 0)
            {
                TempData["ErrorMessage"] = "La durata di estensione deve essere positiva.";
                return RedirectToAction("UserProfile");
            }

            try
            {
                // Converti i minuti in `TimeSpan`
                var extensionDuration = TimeSpan.FromMinutes(extensionDurationInMinutes);

                await _reservationService.ExtendReservation(reservationId, extensionDuration);
                TempData["SuccessMessage"] = "La prenotazione è stata estesa con successo.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Errore durante l'estensione della prenotazione: {ex.Message}";
            }

            return RedirectToAction("UserProfile");
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



        [HttpGet]
        public async Task<IActionResult> InactiveReservations()
        {
            // Recupera tutte le prenotazioni con IsActive impostato su false
            var inactiveReservations = await _reservationService.GetInactiveReservations();

            return View(inactiveReservations);
        }


    }
}
