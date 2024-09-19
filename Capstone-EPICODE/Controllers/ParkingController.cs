using Capstone_EPICODE.Models;
using Capstone_EPICODE.Models.Parcheggio;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone_EPICODE.Controllers
{
    public class ParkingController : Controller
    {
        private readonly IParkingService _parkingService;
        private readonly IAuthService _authService; // Aggiungi il servizio per gestire gli utenti

        public ParkingController(IParkingService parkingService, IAuthService authService)
        {
            _parkingService = parkingService;
            _authService = authService; // Inizializza il servizio utenti
        }

        // Metodo per visualizzare tutti i parcheggi
        public async Task<IActionResult> Index()
        {
            var parkings = await _parkingService.GetAll();
            return View(parkings);  // Mostra tutti i parcheggi
        }

        // Metodo GET per visualizzare il form di creazione
        public async Task<IActionResult> Create()
        {
            // Popoliamo i ParkingManagers per il dropdown
            ViewBag.ParkingManagers = (await _authService.GetAll())
                .Where(u => u.Roles.Any(r => r.Name == "ParkingManager"))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Parking parking, List<int> SelectedManagers)
        {
            if (ModelState.IsValid)
            {
                // Se è stato selezionato almeno un Parking Manager
                if (SelectedManagers != null && SelectedManagers.Any())
                {
                    // Associa il primo Parking Manager selezionato (per semplicità)
                    parking.ParkingManagerId = SelectedManagers.First();
                }

                await _parkingService.Create(parking);
                return RedirectToAction(nameof(Index));
            }

            // Ripopola i ParkingManagers in caso di errore
            ViewBag.ParkingManagers = (await _authService.GetAll())
                .Where(u => u.Roles.Any(r => r.Name == "ParkingManager"))
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            return View(parking);
        }

        // Metodo per visualizzare i dettagli di un parcheggio
        public async Task<IActionResult> Details(int id)
        {
            var parking = await _parkingService.GetById(id);
            if (parking == null)
            {
                return NotFound();
            }
            return View(parking);
        }

        // Metodo per visualizzare il form di modifica di un parcheggio
        public async Task<IActionResult> Edit(int id)
        {
            var parking = await _parkingService.GetById(id);
            if (parking == null)
            {
                return NotFound();
            }
            return View(parking);
        }

        // Metodo POST per aggiornare un parcheggio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Parking parking)
        {
            if (id != parking.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _parkingService.Update(parking);
                return RedirectToAction(nameof(Index));
            }
            return View(parking);
        }

        // Metodo per visualizzare il form di cancellazione di un parcheggio
        public async Task<IActionResult> Delete(int id)
        {
            var parking = await _parkingService.GetById(id);
            if (parking == null)
            {
                return NotFound();
            }
            return View(parking);
        }

        // Metodo POST per confermare la cancellazione di un parcheggio
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _parkingService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
      

        public async Task<IActionResult> AssignParkings()
        {
            var parkingSpots = await _parkingService.GetAll();

            // Ottenere i ParkingManager disponibili usando AuthService
            ViewBag.ParkingManagers = (await _authService.GetAll())  // Recupera tutti gli utenti
                .Where(u => u.Roles.Any(r => r.Name == "ParkingManager"))  // Filtra i ParkingManager
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();

            return View(parkingSpots);
        }
        [HttpGet]
        public async Task<IActionResult> AvailableParkings()
        {
            // Ottieni i parcheggi disponibili
            var parkingList = await _parkingService.GetAll();
            var availableParkings = parkingList.Where(p => p.IsAvailable).ToList();

            return View(availableParkings); // Passa i parcheggi disponibili alla vista
        }


    }
}
