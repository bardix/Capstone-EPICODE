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
            var role = await _roleSvc.Read(id);
            return View(role);
        }

        public async Task<IActionResult> DeleteConfirmedRole(int id)
        {
            await _roleSvc.Delete(id);
            return RedirectToAction("AllRoles", "Auth");
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

                
                if (!roleSelected.Any())
                {
                    var defaultRole = await _roleSvc.GetByName("Utente");
                    roleSelected = new List<int> { defaultRole.Id };
                }


                await _authSvc.Create(user, roleSelected);
                return RedirectToAction("Login", "Auth");
            }
            return View();
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
