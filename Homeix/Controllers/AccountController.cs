using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Homeix.Controllers
{
    public class AccountController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public AccountController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ==========================
        // GET: /Account/Login
        // ==========================
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ==========================
        // POST: /Account/Login
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            string hashedPassword = HashPassword(password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            if (user.Role == null)
            {
                ViewBag.Error = "User role is missing.";
                return View();
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "HomeixAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("HomeixAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            // Redirect safely
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ==========================
        // GET: /Account/Register
        // ==========================
        public IActionResult Register()
        {
            ViewBag.Roles = _context.UserRoles.ToList();
            return View();
        }

        // ==========================
        // POST: /Account/Register
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                ViewBag.Roles = _context.UserRoles.ToList();
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "This email is already registered.";
                ViewBag.Roles = _context.UserRoles.ToList();
                return View();
            }

            var role = await _context.UserRoles.FindAsync(roleId);
            if (role == null)
            {
                ViewBag.Error = "Invalid role selected.";
                ViewBag.Roles = _context.UserRoles.ToList();
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone ?? "",
                RoleId = roleId,
                PasswordHash = HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // ==========================
        // POST: /Account/Logout
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("HomeixAuth");
            return RedirectToAction("Login");
        }

        // ==========================
        // Password Hashing (SHA256)
        // ==========================
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
