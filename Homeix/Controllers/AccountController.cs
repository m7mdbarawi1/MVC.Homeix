using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _env;

        public AccountController(HOMEIXDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =============================================================
        // LOGIN (GET)
        // =============================================================
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =============================================================
        // LOGIN (POST)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            string hashed = HashPassword(password);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hashed);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("UserId", user.UserId.ToString())
            };

            await HttpContext.SignInAsync(
                "HomeixAuth",
                new ClaimsPrincipal(new ClaimsIdentity(claims, "HomeixAuth")),
                new AuthenticationProperties { IsPersistent = true }
            );

            // ✅ 1. Respect returnUrl FIRST
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // ✅ 2. Role-based dashboard redirect
            switch (user.Role.RoleName)
            {
                case "admin":
                    return RedirectToAction("AdminDashboard", "Dashboard");

                case "customer":
                    return RedirectToAction("CustomerDashboard", "Dashboard");

                case "worker":
                    return RedirectToAction("WorkerDashboard", "Dashboard");
            }

            // ✅ 3. Fallback
            return RedirectToAction("Index", "Home");
        }

        // =============================================================
        // REGISTER (GET)
        // =============================================================
        public IActionResult Register()
        {
            ViewBag.Roles = _context.UserRoles.ToList();
            return View();
        }

        // =============================================================
        // REGISTER (POST)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string password, int roleId)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
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
                ViewBag.Error = "Invalid role.";
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

        // =============================================================
        // MY PROFILE (GET)
        // =============================================================
        public async Task<IActionResult> MyProfile()
        {
            var idClaim = User.FindFirst("UserId");
            if (idClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(idClaim.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // =============================================================
        // MY PROFILE (POST)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(User model, string? NewPassword, IFormFile? ProfileImage)
        {
            var dbUser = await _context.Users.FindAsync(model.UserId);
            if (dbUser == null)
                return NotFound();

            if (_context.Users.Any(u => u.Email == model.Email && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(model);
            }

            dbUser.FullName = model.FullName;
            dbUser.Email = model.Email;
            dbUser.PhoneNumber = model.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(NewPassword))
                dbUser.PasswordHash = HashPassword(NewPassword);

            if (ProfileImage != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads/profile");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                string path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await ProfileImage.CopyToAsync(stream);

                dbUser.ProfilePicture = fileName;
            }

            await _context.SaveChangesAsync();

            // Refresh auth
            await HttpContext.SignOutAsync("HomeixAuth");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbUser.FullName),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim(ClaimTypes.Role, (await _context.UserRoles.FindAsync(dbUser.RoleId))!.RoleName),
                new Claim("UserId", dbUser.UserId.ToString())
            };

            await HttpContext.SignInAsync(
                "HomeixAuth",
                new ClaimsPrincipal(new ClaimsIdentity(claims, "HomeixAuth")),
                new AuthenticationProperties { IsPersistent = true }
            );

            ViewBag.Success = "Profile updated successfully.";
            return View(dbUser);
        }

        // =============================================================
        // LOGOUT
        // =============================================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("HomeixAuth");
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync("HomeixAuth");
            return RedirectToAction("Login");
        }

        // =============================================================
        // DELETE ACCOUNT
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var idClaim = User.FindFirst("UserId");
            if (idClaim == null)
                return RedirectToAction("Login");

            int userId = int.Parse(idClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync("HomeixAuth");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult HomeRedirect()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("admin"))
                    return RedirectToAction("AdminDashboard", "Dashboard");

                if (User.IsInRole("customer"))
                    return RedirectToAction("CustomerDashboard", "Dashboard");

                if (User.IsInRole("worker"))
                    return RedirectToAction("WorkerDashboard", "Dashboard");
            }

            // Not logged in → landing page
            return RedirectToAction("Index", "Home");
        }

        // =============================================================
        // PASSWORD HASHING
        // =============================================================
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(password))
            );
        }
    }
}
