using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;


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
        // LOGIN (POST) - BCrypt 
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

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // BCrypt verify only
            if (!VerifyPassword(password, user.PasswordHash))
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

            // Respect returnUrl FIRST
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Role-based dashboard redirect
            return user.Role.RoleName switch
            {
                "admin" => RedirectToAction("AdminDashboard", "Dashboard"),
                "customer" => RedirectToAction("CustomerDashboard", "Dashboard"),
                "worker" => RedirectToAction("WorkerDashboard", "Dashboard"),
                _ => RedirectToAction("Index", "Home")
            };
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
        // REGISTER (POST) - BCrypt 
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone, string password, int roleId)
        {

            if (!IsStrongPassword(password, out var passError))
            {
                ViewBag.Error = passError;
                ViewBag.Roles = _context.UserRoles.ToList();
                return View();
            }

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
                PasswordHash = HashPassword(password), // BCrypt
                ProfilePicture = null
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
        // MY PROFILE (POST) - BCrypt 
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
                dbUser.PasswordHash = HashPassword(NewPassword); // BCrypt

            if (ProfileImage != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads/profile");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

             
                var ext = Path.GetExtension(ProfileImage.FileName);
                string fileName = $"{Guid.NewGuid()}{ext}";
                string path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await ProfileImage.CopyToAsync(stream);

                dbUser.ProfilePicture = fileName;
            }

            await _context.SaveChangesAsync();

            // Refresh auth
            await HttpContext.SignOutAsync("HomeixAuth");

            var role = await _context.UserRoles.FindAsync(dbUser.RoleId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dbUser.FullName),
                new Claim(ClaimTypes.Email, dbUser.Email),
                new Claim(ClaimTypes.Role, role!.RoleName),
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

        // =============================================================
        // HOME REDIRECT
        // =============================================================
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

            return RedirectToAction("Index", "Home");
        }

        // =============================================================
        // PASSWORD HASHING / VERIFY (BCrypt)
        // =============================================================
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            // BCrypt hashes start with $2a$ / $2b$ / $2y$
            if (!(storedHash.StartsWith("$2a$") || storedHash.StartsWith("$2b$") || storedHash.StartsWith("$2y$")))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        private bool IsStrongPassword(string password, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Password is required.";
                return false;
            }

            if (password.Length < 8)
            {
                error = "Password must be at least 8 characters.";
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => "!@#$%^&*()_+-=[]{};':\",.<>/?\\|`~".Contains(ch));

            if (!hasUpper) { error = "Password must contain at least 1 uppercase letter."; return false; }
            if (!hasLower) { error = "Password must contain at least 1 lowercase letter."; return false; }
            if (!hasDigit) { error = "Password must contain at least 1 number."; return false; }
            if (!hasSpecial) { error = "Password must contain at least 1 special character."; return false; }

            return true;
        }

    }
}
