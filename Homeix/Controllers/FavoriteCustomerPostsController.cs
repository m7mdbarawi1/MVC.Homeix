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
    public class FavoriteCustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public FavoriteCustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // MY FAVORITES
        // =========================
        public async Task<IActionResult> Index()
        {
            int userId = GetUserId();

            var favorites = await _context.FavoriteCustomerPosts
                .Where(f => f.UserId == userId)
                .Include(f => f.CustomerPost)
                    .ThenInclude(p => p.PostCategory)
                .Include(f => f.CustomerPost)
                    .ThenInclude(p => p.Media)
                .OrderByDescending(f => f.FavoritePostId)
                .ToListAsync();

            return View(favorites);
        }

        // =========================
        // ADD TO FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int customerPostId)
        {
            int userId = GetUserId();

            bool exists = await _context.FavoriteCustomerPosts
                .AnyAsync(f => f.UserId == userId && f.CustomerPostId == customerPostId);

            if (!exists)
            {
                _context.FavoriteCustomerPosts.Add(new FavoriteCustomerPost
                {
                    UserId = userId,
                    CustomerPostId = customerPostId
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "CustomerPosts", new { id = customerPostId });
        }

        // =========================
        // REMOVE FROM FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int customerPostId)
        {
            int userId = GetUserId();

            var favorite = await _context.FavoriteCustomerPosts
                .FirstOrDefaultAsync(f =>
                    f.UserId == userId &&
                    f.CustomerPostId == customerPostId);

            if (favorite != null)
            {
                _context.FavoriteCustomerPosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HELPERS
        // =========================
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null)
                throw new UnauthorizedAccessException();

            return int.Parse(claim.Value);
        }
    }
}
