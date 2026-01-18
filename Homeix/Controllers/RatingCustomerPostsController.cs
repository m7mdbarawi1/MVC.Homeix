using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class RatingCustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public RatingCustomerPostsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: RatingCustomerPosts
        // ========================
        public async Task<IActionResult> Index()
        {
            var ratings = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RaterUser)
                .Include(r => r.RatedUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(ratings);
        }

        // ========================
        // DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var ratings = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RaterUser)
                .Include(r => r.RatedUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RatingId,RatingValue,Review,CreatedAt,JobProgressId,RatedUser,RaterUser");

            foreach (var r in ratings)
            {
                sb.AppendLine(
                    $"{r.RatingCustomerPostId}," +
                    $"{r.RatingValue}," +
                    $"\"{r.Review}\"," +
                    $"{r.CreatedAt:yyyy-MM-dd}," +
                    $"{r.JobProgressId}," +
                    $"\"{r.RatedUser?.FullName}\"," +
                    $"\"{r.RaterUser?.FullName}\""
                );
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "RatingCustomerPostsReport.csv");
        }

        // ========================
        // GET: RatingCustomerPosts/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RaterUser)
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.RatingCustomerPostId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // GET: RatingCustomerPosts/Create
        // ========================
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // ========================
        // POST: RatingCustomerPosts/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            RatingCustomerPost rating)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(rating);
                return View(rating);
            }

            rating.CreatedAt = DateTime.Now;

            _context.RatingCustomerPosts.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: RatingCustomerPosts/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts.FindAsync(id);
            if (rating == null)
                return NotFound();

            LoadDropdowns(rating);
            return View(rating);
        }

        // ========================
        // POST: RatingCustomerPosts/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("RatingCustomerPostId,JobProgressId,RaterUserId,RatedUserId,RatingValue,Review")]
            RatingCustomerPost rating)
        {
            if (id != rating.RatingCustomerPostId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadDropdowns(rating);
                return View(rating);
            }

            var existing = await _context.RatingCustomerPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RatingCustomerPostId == id);

            if (existing == null)
                return NotFound();

            rating.CreatedAt = existing.CreatedAt;

            _context.Update(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: RatingCustomerPosts/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var rating = await _context.RatingCustomerPosts
                .Include(r => r.JobProgress)
                .Include(r => r.RaterUser)
                .Include(r => r.RatedUser)
                .FirstOrDefaultAsync(r => r.RatingCustomerPostId == id);

            if (rating == null)
                return NotFound();

            return View(rating);
        }

        // ========================
        // POST: RatingCustomerPosts/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _context.RatingCustomerPosts.FindAsync(id);
            if (rating != null)
            {
                _context.RatingCustomerPosts.Remove(rating);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadDropdowns(RatingCustomerPost? rating = null)
        {
            ViewData["JobProgressId"] = new SelectList(
                _context.JobProgresses,
                "JobProgressId",
                "JobProgressId",
                rating?.JobProgressId
            );

            ViewData["RaterUserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName",
                rating?.RaterUserId
            );

            ViewData["RatedUserId"] = new SelectList(
                _context.Users,
                "UserId",
                "FullName",
                rating?.RatedUserId
            );
        }
    }
}
