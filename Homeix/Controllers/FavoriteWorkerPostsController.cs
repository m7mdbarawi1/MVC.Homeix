using System;
using System.Linq;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class FavoriteWorkerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoriteWorkerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // ADD TO FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int workerPostId, string returnUrl)
        {
            int userId = GetUserId();

            bool exists = await _context.FavoriteWorkerPosts
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.WorkerPostId == workerPostId);

            if (!exists)
            {
                _context.FavoriteWorkerPosts.Add(new FavoriteWorkerPost
                {
                    UserId = userId,
                    WorkerPostId = workerPostId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectBack(returnUrl);
        }

        // =========================
        // REMOVE FROM FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int workerPostId, string returnUrl)
        {
            int userId = GetUserId();

            var favorite = await _context.FavoriteWorkerPosts
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.WorkerPostId == workerPostId);

            if (favorite != null)
            {
                _context.FavoriteWorkerPosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectBack(returnUrl);
        }

        // =========================
        // REDIRECT BACK TO SAME PAGE
        // =========================
        private IActionResult RedirectBack(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // USER ID HELPER
        // =========================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException("UserId claim not found.");

            return int.Parse(claim.Value);
        }
    }
}
