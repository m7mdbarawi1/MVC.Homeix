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
        // ADD TO FAVORITES
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int customerPostId, string returnUrl)
        {
            int userId = GetUserId();

            bool exists = await _context.FavoriteCustomerPosts
                .AnyAsync(f =>
                    f.UserId == userId &&
                    f.CustomerPostId == customerPostId);

            if (!exists)
            {
                _context.FavoriteCustomerPosts.Add(new FavoriteCustomerPost
                {
                    UserId = userId,
                    CustomerPostId = customerPostId
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
        public async Task<IActionResult> Remove(int customerPostId, string returnUrl)
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
