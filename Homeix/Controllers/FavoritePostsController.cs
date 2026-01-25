using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Homeix.Controllers
{
    [Authorize]
    public class FavoritePostsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public FavoritePostsController(HOMEIXDbContext context){_context = context;}
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var favorites = await _context.FavoritePosts.Include(f => f.User).OrderByDescending(f => f.AddedAt).ToListAsync();
            return View(favorites);
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var favorites = await _context.FavoritePosts.Include(f => f.User).OrderByDescending(f => f.AddedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("FavoritePostId,PostType,PostId,AddedAt,UserId");
            foreach (var f in favorites)
            {
                sb.AppendLine($"{f.FavoritePostId}," + $"{f.PostType}," + $"{f.PostId}," + $"{f.AddedAt:yyyy-MM-dd HH:mm}," + $"{f.UserId}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "FavoritePostsReport.csv");
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkerPost(int postId)
        {
            int userId = GetUserId();
            bool exists = await _context.FavoritePosts.AnyAsync(f => f.UserId == userId && f.PostType == "WorkerPost" && f.PostId == postId);
            if (!exists)
            {
                _context.FavoritePosts.Add(new FavoritePost{UserId = userId, PostType = "WorkerPost", PostId = postId, AddedAt = DateTime.Now});
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CustomerDashboard", "Dashboard");
        }
        [HttpPost]
        [Authorize(Roles = "customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWorkerPost(int postId)
        {
            int userId = GetUserId();
            var favorite = await _context.FavoritePosts.FirstOrDefaultAsync(f => f.UserId == userId && f.PostType == "WorkerPost" && f.PostId == postId);
            if (favorite != null)
            {
                _context.FavoritePosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("CustomerDashboard", "Dashboard");
        }
        [HttpPost]
        [Authorize(Roles = "worker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomerPost(int postId)
        {
            int userId = GetUserId();
            bool exists = await _context.FavoritePosts.AnyAsync(f => f.UserId == userId && f.PostType == "CustomerPost" && f.PostId == postId);
            if (!exists)
            {
                _context.FavoritePosts.Add(new FavoritePost {UserId = userId, PostType = "CustomerPost", PostId = postId, AddedAt = DateTime.Now});
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("WorkerDashboard", "Dashboard");
        }
        [HttpPost]
        [Authorize(Roles = "worker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCustomerPost(int postId)
        {
            int userId = GetUserId();
            var favorite = await _context.FavoritePosts.FirstOrDefaultAsync(f => f.UserId == userId && f.PostType == "CustomerPost" &&  f.PostId == postId);
            if (favorite != null)
            {
                _context.FavoritePosts.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("WorkerDashboard", "Dashboard");
        }
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException();
            return int.Parse(claim.Value);
        }
        private void LoadUsersDropdown(int? selectedUserId = null){ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", selectedUserId);}
    }
}
