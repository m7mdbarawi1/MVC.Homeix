using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class CustomerPostsController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public CustomerPostsController(HOMEIXDbContext context) {_context = context;}
        public async Task<IActionResult> Index()
        {
            var posts = await _context.CustomerPosts.Include(p => p.PostCategory).Include(p => p.User).OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }
        public async Task<IActionResult> DownloadReport()
        {
            var posts = await _context.CustomerPosts.Include(p => p.PostCategory).Include(p => p.User).OrderByDescending(p => p.CreatedAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("PostId,Title,Category,User,Location,MinPrice,MaxPrice,Status,IsActive,CreatedAt");
            foreach (var post in posts)
            {
                sb.AppendLine(
                    $"{post.CustomerPostId}," +
                    $"\"{post.Title}\"," +
                    $"\"{post.PostCategory?.CategoryName}\"," +
                    $"\"{post.User?.FullName}\"," +
                    $"\"{post.Location}\"," +
                    $"{post.PriceRangeMin}," +
                    $"{post.PriceRangeMax}," +
                    $"{post.Status}," +
                    $"{post.IsActive}," +
                    $"{post.CreatedAt:yyyy-MM-dd}"
                );
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "CustomerPostsReport.csv");
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.CustomerPosts.Include(p => p.PostCategory).Include(p => p.User).FirstOrDefaultAsync(p => p.CustomerPostId == id);
            if (post == null) return NotFound();
            var mediaList = await _context.PostMedia.Where(m => m.PostType == "CustomerPost" && m.PostId == id).OrderByDescending(m => m.UploadedAt).ToListAsync();
            ViewBag.PostMedia = mediaList;
            return View(post);
        }
        public IActionResult Create()
        {
            LoadDropdowns();
            ViewBag.LoggedInUserId = GetUserId();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax")] CustomerPost customerPost, List<IFormFile>? mediaFiles)
        {
            customerPost.UserId = GetUserId();
            ModelState.Remove(nameof(CustomerPost.UserId));

            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                ViewBag.LoggedInUserId = customerPost.UserId;
                return View(customerPost);
            }

            customerPost.CreatedAt = DateTime.Now;
            customerPost.Status = "Open";
            customerPost.IsActive = true;

            _context.CustomerPosts.Add(customerPost);
            await _context.SaveChangesAsync();

            var postedFiles = Request.Form.Files.Where(f => f.Name == "mediaFiles" && f.Length > 0).ToList();
            if (postedFiles.Count == 0 && mediaFiles != null)
                postedFiles = mediaFiles.Where(f => f != null && f.Length > 0).ToList();
            if (postedFiles.Count > 0)
            {
                foreach (var file in postedFiles)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(extension)) continue;
                    var savedPath = await SaveCustomerPostMediaAsync(file);
                    _context.PostMedia.Add(new PostMedium
                    {
                        PostType = "CustomerPost",
                        PostId = customerPost.CustomerPostId,
                        MediaPath = savedPath,
                        UploadedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MyPosts));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)  return NotFound();
            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null)  return NotFound();
            LoadDropdowns(post);
            var mediaList = await _context.PostMedia.Where(m => m.PostType == "CustomerPost" && m.PostId == post.CustomerPostId).OrderByDescending(m => m.UploadedAt).ToListAsync();
            ViewBag.PostMedia = mediaList;
            return View(post);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerPostId,PostCategoryId,Title,Description,Location,PriceRangeMin,PriceRangeMax,IsActive")] CustomerPost customerPost, List<IFormFile>? newMediaFiles, int[]? deleteMediaIds)
        {
            if (id != customerPost.CustomerPostId) return NotFound();
            var existing = await _context.CustomerPosts.AsNoTracking().FirstOrDefaultAsync(p => p.CustomerPostId == id);
            if (existing == null) return NotFound();
            if (!ModelState.IsValid)
            {
                LoadDropdowns(customerPost);
                var mediaList = await _context.PostMedia.Where(m => m.PostType == "CustomerPost" && m.PostId == id).OrderByDescending(m => m.UploadedAt).ToListAsync();
                ViewBag.PostMedia = mediaList;
                return View(customerPost);
            }
            customerPost.UserId = existing.UserId;
            customerPost.CreatedAt = existing.CreatedAt;
            customerPost.Status = existing.Status;
            _context.CustomerPosts.Update(customerPost);
            if (deleteMediaIds != null && deleteMediaIds.Length > 0)
            {
                var mediaToDelete = await _context.PostMedia.Where(m => deleteMediaIds.Contains(m.MediaId) && m.PostType == "CustomerPost"&& m.PostId == id).ToListAsync();
                foreach (var media in mediaToDelete)
                {
                    DeletePhysicalFile(media.MediaPath);
                    _context.PostMedia.Remove(media);
                }
            }
            if (newMediaFiles != null && newMediaFiles.Any(f => f != null && f.Length > 0))
            {
                foreach (var file in newMediaFiles.Where(f => f != null && f.Length > 0))
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(extension)) continue;
                    var savedPath = await SaveCustomerPostMediaAsync(file);
                    _context.PostMedia.Add(new PostMedium
                    {PostType = "CustomerPost", PostId = id, MediaPath = savedPath,UploadedAt = DateTime.Now});
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyPosts));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var post = await _context.CustomerPosts.Include(p => p.PostCategory).Include(p => p.User).FirstOrDefaultAsync(p => p.CustomerPostId == id);
            if (post == null) return NotFound();
            return View(post);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.CustomerPosts.FindAsync(id);
            if (post == null) return NotFound();
            var media = await _context.PostMedia.Where(m => m.PostType == "CustomerPost" && m.PostId == id).ToListAsync();
            foreach (var m in media)
            {
                DeletePhysicalFile(m.MediaPath);
                _context.PostMedia.Remove(m);
            }
            _context.CustomerPosts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyPosts));
        }
        public async Task<IActionResult> MyPosts()
        {
            int userId = GetUserId();

            var posts = await _context.CustomerPosts
                .Where(p => p.UserId == userId)
                .Include(p => p.PostCategory)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var postIds = posts.Select(p => p.CustomerPostId).ToList();

            var mediaLookup = await _context.PostMedia
                .Where(m => m.PostType == "CustomerPost" && postIds.Contains(m.PostId))
                .GroupBy(m => m.PostId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            ViewBag.PostMedia = mediaLookup;

            return View(posts);
        }
        private int GetUserId()
        {
            var claim = User.FindFirst("UserId");
            if (claim == null) throw new UnauthorizedAccessException();
            return int.Parse(claim.Value);
        }
        private void LoadDropdowns(CustomerPost? post = null) {ViewData["PostCategoryId"] = new SelectList(_context.PostCategories.AsNoTracking().OrderBy(c => c.CategoryName), "PostCategoryId","CategoryName",post?.PostCategoryId);}
        private static readonly string[] AllowedImageExtensions = {".jpg", ".jpeg", ".png", ".gif", ".webp"};
        private async Task<string> SaveCustomerPostMediaAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "uploads", "post-media");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create)) {await file.CopyToAsync(stream);}
            return "/uploads/post-media/" + fileName;
        }
        private void DeletePhysicalFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
        }
    }
}