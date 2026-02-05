using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Models;
using Homeix.Data;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public UsersController(HOMEIXDbContext context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("UserId,FullName,Email,PhoneNumber,Role,HasProfileImage");

            foreach (var u in users)
            {
                sb.AppendLine(
                    $"{u.UserId}," +
                    $"\"{u.FullName}\"," +
                    $"\"{u.Email}\"," +
                    $"\"{u.PhoneNumber}\"," +
                    $"\"{u.Role?.RoleName}\"," +
                    $"{(!string.IsNullOrWhiteSpace(u.ProfilePicture))}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "UsersReport.csv"
            );
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.RatingRatedUsers)
                    .ThenInclude(r => r.RaterUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();
            return View(user);
        }

        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            LoadRolesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(int roleId, string fullName, string email,string phoneNumber,string password,IFormFile? profileImage)
        {
            ValidateUserInput(roleId, fullName, email, phoneNumber, password);

            if (await _context.Users.AnyAsync(u => u.Email == email))
                ModelState.AddModelError("Email", "Email already exists.");

            if (!ModelState.IsValid)
            {
                LoadRolesDropdown(roleId);
                return View();
            }

            string? imagePath = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                imagePath = await SaveProfileImageAsync(profileImage);
            }

            var user = new User
            {
                RoleId = roleId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = HashPassword(password),
                ProfilePicture = imagePath
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            LoadRolesDropdown(user.RoleId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id,int roleId,string fullName,string email,string phoneNumber,IFormFile? profileImage)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id))
                ModelState.AddModelError("Email", "Email already exists.");

            if (!ModelState.IsValid)
            {
                LoadRolesDropdown(roleId);
                return View(user);
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                DeletePhysicalFile(user.ProfilePicture);
                user.ProfilePicture = await SaveProfileImageAsync(profileImage);
            }

            user.RoleId = roleId;
            user.FullName = fullName;
            user.Email = email;
            user.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                DeletePhysicalFile(user.ProfilePicture);
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadRolesDropdown(int? selectedRoleId = null)
        {
            ViewData["RoleId"] = new SelectList(
                _context.UserRoles.AsNoTracking().OrderBy(r => r.RoleName),
                "RoleId",
                "RoleName",
                selectedRoleId
            );
        }

        private void ValidateUserInput(int roleId,string fullName,string email,string phoneNumber,string password)
        {
            if (roleId <= 0)
                ModelState.AddModelError("RoleId", "Role is required.");

            if (string.IsNullOrWhiteSpace(fullName))
                ModelState.AddModelError("FullName", "Full name is required.");

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("Email", "Email is required.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required.");

            if (!IsStrongPassword(password, out var error))
                ModelState.AddModelError("Password", error);
        }
        [Authorize]
        public async Task<IActionResult> PublicProfile(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.RatingRatedUsers)
                    .ThenInclude(r => r.RaterUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        private static async Task<string> SaveProfileImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                throw new InvalidOperationException("Invalid image type.");

            var uploadDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "profile");

            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/profile/" + fileName;
        }

        private static void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            if (relativePath.StartsWith("/images/")) return;

            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                relativePath.TrimStart('/'));

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private bool IsStrongPassword(string password, out string error)
        {
            error = "";

            if (password.Length < 8) { error = "Password must be at least 8 characters."; return false; }
            if (!password.Any(char.IsUpper)) { error = "Password must contain an uppercase letter."; return false; }
            if (!password.Any(char.IsLower)) { error = "Password must contain a lowercase letter."; return false; }
            if (!password.Any(char.IsDigit)) { error = "Password must contain a number."; return false; }
            if (!password.Any(ch => "!@#$%^&*()_+-=[]{};':\",.<>/?\\|`~".Contains(ch)))
            { error = "Password must contain a special character."; return false; }

            return true;
        }
    }
}
