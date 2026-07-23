using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoCaptain.Data;
using CargoCaptain.Models;
using CargoCaptain.ViewModels;
using CargoCaptain.Enums;

namespace CargoCaptain.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectUserByRole(User.FindFirst(ClaimTypes.Role)?.Value);
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find login based on username or AssociatedName matching
            var login = await _context.Logins
                .FirstOrDefaultAsync(l => l.AssociatedName == model.Username || l.UserId.ToString() == model.Username);

            if (login == null)
            {
                // Fallback: search email via Employee table if it matches
                var employee = await _context.Employees
                    .Include(e => e.Login)
                    .FirstOrDefaultAsync(e => e.email == model.Username);
                
                if (employee != null)
                {
                    login = employee.Login;
                }
            }

            if (login != null)
            {
                var hasher = new PasswordHasher<Login>();
                var verifyResult = hasher.VerifyHashedPassword(login, login.Password, model.Password);

                if (verifyResult == PasswordVerificationResult.Success)
                {
                    // Create Claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, login.AssociatedName),
                        new Claim(ClaimTypes.Role, login.Role.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, login.UserId.ToString()),
                        new Claim("UserId", login.UserId.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                    return RedirectUserByRole(login.Role.ToString());
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect("/");
            }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Strict Validation: Only Shipper and Consignee can register
            if (model.Role != UserRole.Shipper && model.Role != UserRole.Consignee)
            {
                ModelState.AddModelError("Role", "Only Shipper and Consignee roles can register online.");
                return View(model);
            }

            // Check if email or username already exists
            var existingLogin = await _context.Logins
                .AnyAsync(l => l.AssociatedName == model.FullName);
            if (existingLogin)
            {
                ModelState.AddModelError(string.Empty, "An account with that name already exists.");
                return View(model);
            }

            // Create new Login entity
            var newLogin = new Login
            {
                AssociatedName = model.FullName,
                Role = model.Role,
            };

            var hasher = new PasswordHasher<Login>();
            newLogin.Password = hasher.HashPassword(newLogin, model.Password);

            _context.Logins.Add(newLogin);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public new IActionResult Unauthorized()
        {
            return View();
        }

        private IActionResult RedirectUserByRole(string? role)
        {
            return role switch
            {
                nameof(UserRole.Admin) => Redirect("/Admin/Dashboard"),
                nameof(UserRole.FreightForwarder) => Redirect("/FreightForwarder/Dashboard"),
                nameof(UserRole.CustomsBroker) => Redirect("/CustomsBroker/Dashboard"),
                nameof(UserRole.PortOperator) => Redirect("/PortOperator/Dashboard"),
                nameof(UserRole.Shipper) => Redirect("/Shipper/Dashboard"),
                nameof(UserRole.Consignee) => Redirect("/Consignee/Dashboard"),
                _ => Redirect("/")
            };
        }
    }
}
