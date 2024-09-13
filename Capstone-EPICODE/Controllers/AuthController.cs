using Capstone_EPICODE.Models;
using Capstone_EPICODE.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Capstone_EPICODE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IRoleService _roleSvc;
        private readonly IAuthService _authSvc;

        public AuthController(IRoleService roleSvc, IAuthService authSvc)
        {
            _roleSvc = roleSvc;
            _authSvc = authSvc;
        }

        // Ruoli
        public async Task<IActionResult> AllRoles()
        {
            var roles = await _roleSvc.GetAll();
            return View(roles);
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (ModelState.IsValid)
            {
                await _roleSvc.Create(role);
                return RedirectToAction("AllRoles", "Auth");
            }
            return View(role);
        }

        public async Task<IActionResult> EditRole(int id)
        {
            var role = await _roleSvc.Read(id);
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(Role role)
        {
            if (ModelState.IsValid)
            {
                await _roleSvc.Update(role);
                return RedirectToAction("AllRoles", "Auth");
            }
            return View(role);
        }

        public async Task<IActionResult> DeleteRole(int id)
        {
            // Recupera il ruolo da eliminare utilizzando il servizio
            var role = await _roleSvc.Read(id);

            // Se il ruolo non esiste, mostra un errore o gestisci l'eccezione
            if (role == null)
            {
                return NotFound();
            }

            return View(role);  // Mostra la vista per confermare l'eliminazione
        }

        public async Task<IActionResult> DeleteConfirmedRole(int id)
        {
            try
            {
                // Chiama il metodo Delete del servizio per eliminare il ruolo
                await _roleSvc.Delete(id);
                return RedirectToAction("AllRoles", "Auth");  // Dopo l'eliminazione, reindirizza alla lista dei ruoli
            }
            catch (Exception ex)
            {
                // Gestione degli errori, ad esempio se il ruolo è associato a utenti
                ModelState.AddModelError("", ex.Message);

                // Ricarica il ruolo da eliminare per visualizzare di nuovo la conferma
                var role = await _roleSvc.Read(id);
                if (role == null)
                {
                    return NotFound();
                }

                return View("DeleteRole", role);  // Ritorna alla vista di eliminazione con il messaggio di errore
            }
        }


        // Registrazione
        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            var roles = await _roleSvc.GetAll();
            ViewBag.Roles = roles.Select(role => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name
            }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserViewModel user, IEnumerable<int> roleSelected)
        {
            if (ModelState.IsValid)
            {
                // Controlla se l'utente ha selezionato uno o più ruoli
                if (!roleSelected.Any())
                {
                    // Se non sono stati selezionati ruoli, assegna il ruolo di default "User"
                    var defaultRole = await _roleSvc.GetByName("User");  // Recupera il ruolo di default dal servizio
                    roleSelected = new List<int> { defaultRole.Id };     // Assegna il ruolo di "User"
                }

                // Passiamo i ruoli selezionati o il ruolo di default al servizio
                await _authSvc.Create(user, roleSelected);
                return RedirectToAction("Login", "Auth");
            }

            // Ricarica i ruoli nel caso di errore nella registrazione
            var roles = await _roleSvc.GetAll();
            ViewBag.Roles = roles.Select(role => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name
            }).ToList();

            return View(user);
        }


        // Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserViewModel user)
        {
            var u = await _authSvc.Login(user);
            if (u != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, u.Name),
                };
                u.Roles.ForEach(r => claims.Add(new Claim(ClaimTypes.Role, r.Name)));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(user);
        }

        // Logout
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Gestione utenti
        public async Task<IActionResult> AllUsers()
        {
            var users = await _authSvc.GetAll();
            return View(users);
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _authSvc.GetById(id);
            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            await _authSvc.Delete(id);
            return RedirectToAction("AllUsers", "Auth");
        }

        // Dettagli utente e gestione ruoli
        public async Task<IActionResult> DetailUser(int id)
        {
            var user = await _authSvc.GetById(id);
            var userRoles = user.Roles.Select(r => r.Name).ToList();
            var allRoles = await _roleSvc.GetAll();
            var rolesName = allRoles.Select(role => role.Name).ToList();
            var rolesToRemove = rolesName.Where(r => userRoles.Contains(r)).ToList();

            rolesName.RemoveAll(item => rolesToRemove.Contains(item));

            ViewBag.Roles = rolesName;
            return View(user);
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _authSvc.GetById(id);  // Recupera l'utente con i ruoli
            if (user == null)
            {
                return NotFound();
            }

            // Recupera tutti i ruoli
            var allRoles = await _roleSvc.GetAll();

            // Trova i ruoli non ancora assegnati all'utente
            var availableRoles = allRoles.Where(role => !user.Roles.Any(ur => ur.Id == role.Id))
                                          .Select(role => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                                          {
                                              Value = role.Id.ToString(),
                                              Text = role.Name
                                          }).ToList();

            ViewBag.AvailableRoles = availableRoles;  // Passa i ruoli non assegnati alla vista

            return View(user);  // Passa l'utente alla vista
        }

        public async Task<IActionResult> AddRoleToUser(int userid, string roleName)
        {
            await _authSvc.AddRoleToUser(userid, roleName);
            return RedirectToAction("AllUsers", "Auth");
        }

        public async Task<IActionResult> RemoveRoleToUser(int userid, string roleName)
        {
            await _authSvc.RemoveRoleToUser(userid, roleName);
            return RedirectToAction("AllUsers", "Auth");
        }
    }
}
