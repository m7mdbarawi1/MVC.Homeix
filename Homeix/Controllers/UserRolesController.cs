using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public UserRolesController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: UserRoles
        // ========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserRoles.ToListAsync());
        }

        // ========================
        // DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var roles = await _context.UserRoles.ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RoleId,RoleName");

            foreach (var role in roles)
            {
                sb.AppendLine($"{role.RoleId},\"{role.RoleName}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "UserRolesReport.csv");
        }

        // ========================
        // GET: UserRoles/Details
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var role = await _context.UserRoles
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null)
                return NotFound();

            return View(role);
        }

        // ========================
        // GET: UserRoles/Create
        // ========================
        public IActionResult Create()
        {
            return View();
        }

        // ========================
        // POST: UserRoles/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("RoleName")] UserRole userRole)
        {
            if (!ModelState.IsValid)
                return View(userRole);

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: UserRoles/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var role = await _context.UserRoles.FindAsync(id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        // ========================
        // POST: UserRoles/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RoleId,RoleName")] UserRole userRole)
        {
            if (id != userRole.RoleId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(userRole);

            _context.Update(userRole);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: UserRoles/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var role = await _context.UserRoles
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null)
                return NotFound();

            return View(role);
        }

        // ========================
        // POST: UserRoles/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.UserRoles.FindAsync(id);
            if (role != null)
            {
                _context.UserRoles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
