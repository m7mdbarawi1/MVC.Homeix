using Homeix.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public DashboardController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ================= ADMIN =================
        [Authorize(Roles = "admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // ================= CUSTOMER =================
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CustomerDashboard()
        {
            var workerPosts = await _context.WorkerPosts
                .Include(w => w.PostCategory)
                .Include(w => w.User)
                .Where(w => w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            return View(workerPosts);
        }

        // ================= WORKER =================
        [Authorize(Roles = "worker")]
        public async Task<IActionResult> WorkerDashboard()
        {
            var customerPosts = await _context.CustomerPosts
                .Include(c => c.PostCategory)
                .Include(c => c.User)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(customerPosts);
        }
    }
}
