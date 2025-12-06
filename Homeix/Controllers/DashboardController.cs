using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Homeix.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        // ================= ADMIN =================
        [Authorize(Roles = "admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // ================= CUSTOMER =================
        [Authorize(Roles = "customer")]
        public IActionResult CustomerDashboard()
        {
            return View();
        }

        // ================= WORKER =================
        [Authorize(Roles = "worker")]
        public IActionResult WorkerDashboard()
        {
            return View();
        }
    }
}
