using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Models;
using Homeix.Data;

namespace Homeix.Controllers
{
    public class UsersController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public UsersController(HOMEIXDbContext context) {_context = context;}
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users .Include(u => u.Role).AsNoTracking().ToListAsync();
            return View(users);
        }
        public async Task<IActionResult> DownloadReport()
        {
            var users = await _context.Users.Include(u => u.Role).AsNoTracking().ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("UserId,FullName,Email,PhoneNumber,Role");
            foreach (var u in users)
            {
                sb.AppendLine($"{u.UserId}," + $"\"{u.FullName}\"," + $"\"{u.Email}\"," + $"\"{u.PhoneNumber}\"," + $"\"{u.Role?.RoleName}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "UsersReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }
        public IActionResult Create()
        {
            LoadRolesDropdown();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roleId, string fullName, string email, string phoneNumber, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName)) ModelState.AddModelError("FullName", "Full name is required.");
            if (string.IsNullOrWhiteSpace(email)) ModelState.AddModelError("Email", "Email is required.");
            if (string.IsNullOrWhiteSpace(phoneNumber)) ModelState.AddModelError("PhoneNumber", "Phone number is required.");
            if (string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("Password", "Password is required.");
            if (!IsStrongPassword(password, out var passError))
            {
                ModelState.AddModelError("Password", passError);
                LoadRolesDropdown(roleId);
                return View();
            }
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists) ModelState.AddModelError("Email", "Email already exists.");
            if (!ModelState.IsValid)
            {
                LoadRolesDropdown(roleId);
                return View();
            }
            var user = new User {RoleId = roleId, FullName = fullName, Email = email, PhoneNumber = phoneNumber, PasswordHash = HashPassword(password), ProfilePicture = null};
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            LoadRolesDropdown(user.RoleId);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int roleId, string fullName, string email, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (string.IsNullOrWhiteSpace(fullName)) ModelState.AddModelError("FullName", "Full name is required.");
            if (string.IsNullOrWhiteSpace(email)) ModelState.AddModelError("Email", "Email is required.");
            if (string.IsNullOrWhiteSpace(phoneNumber)) ModelState.AddModelError("PhoneNumber", "Phone number is required.");
            var emailUsed = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id);
            if (emailUsed) ModelState.AddModelError("Email", "Email already exists.");
            if (!ModelState.IsValid)
            {
                LoadRolesDropdown(roleId);
                return View(user);
            }
            user.RoleId = roleId;
            user.FullName = fullName;
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.Include(u => u.Role).AsNoTracking().FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private void LoadRolesDropdown(int? selectedRoleId = null) {ViewData["RoleId"] = new SelectList(_context.UserRoles.AsNoTracking().OrderBy(r => r.RoleName), "RoleId", "RoleName", selectedRoleId);}
        private string HashPassword(string password) {return BCrypt.Net.BCrypt.HashPassword(password);}
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

        public async Task<IActionResult> PublicProfile(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

    }
}
