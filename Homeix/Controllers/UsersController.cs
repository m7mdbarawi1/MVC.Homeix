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

        public UsersController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            LoadRolesDropdown();
            return View();
        }

        // POST: Users/Create
        // NOTE: We do NOT bind UserId. It is IDENTITY.
        // NOTE: We do NOT accept PasswordHash from UI. We accept a plain password and hash it.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int roleId, string fullName, string email, string phoneNumber, string password)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(fullName))
                ModelState.AddModelError("FullName", "Full name is required.");

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("Email", "Email is required.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required.");

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("Password", "Password is required.");

            // unique email check
            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailExists)
                ModelState.AddModelError("Email", "Email already exists.");

            if (!ModelState.IsValid)
            {
                LoadRolesDropdown(roleId);
                return View();
            }

            var user = new User
            {
                RoleId = roleId,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = HashPassword(password), //  BCrypt hash
                ProfilePicture = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            LoadRolesDropdown(user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        // NOTE: We load the existing user, then update only allowed fields.
        // NOTE: We do NOT edit PasswordHash here.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int roleId, string fullName, string email, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(fullName))
                ModelState.AddModelError("FullName", "Full name is required.");

            if (string.IsNullOrWhiteSpace(email))
                ModelState.AddModelError("Email", "Email is required.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required.");

            // unique email check (ignore current user)
            var emailUsed = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id);
            if (emailUsed)
                ModelState.AddModelError("Email", "Email already exists.");

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

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
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

        // ------------------------
        // Helpers
        // ------------------------
        private void LoadRolesDropdown(int? selectedRoleId = null)
        {
            ViewData["RoleId"] = new SelectList(
                _context.UserRoles.AsNoTracking().OrderBy(r => r.RoleName),
                "RoleId",
                "RoleName",   //show RoleName 
                selectedRoleId
            );
        }

        // Replace this with your real secure hashing method (BCrypt recommended)
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

       
        }
    }

