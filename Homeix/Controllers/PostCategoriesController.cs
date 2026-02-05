using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    public class PostCategoriesController : Controller
    {
        private readonly HOMEIXDbContext _context;
        
        public PostCategoriesController(HOMEIXDbContext context) {_context = context;}
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.PostCategories.ToListAsync());
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var categories = await _context.PostCategories.OrderBy(c => c.CategoryName).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PostCategoryId,CategoryName");
            foreach (var c in categories)
            {
                sb.AppendLine($"{c.PostCategoryId},\"{c.CategoryName}\"");
            }
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "PostCategoriesReport.csv");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var postCategory = await _context.PostCategories.FirstOrDefaultAsync(m => m.PostCategoryId == id);
            if (postCategory == null)return NotFound();
            return View(postCategory);
        }

        [Authorize(Roles = "admin")]
        public IActionResult Create(){return View();}
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("CategoryName")] PostCategory postCategory)
        {
            if (!ModelState.IsValid) return View(postCategory);
            _context.PostCategories.Add(postCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var postCategory = await _context.PostCategories.FindAsync(id);
            if (postCategory == null) return NotFound();
            return View(postCategory);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("PostCategoryId,CategoryName")] PostCategory postCategory)
        {
            if (id != postCategory.PostCategoryId) return NotFound();
            if (!ModelState.IsValid) return View(postCategory);
            _context.Update(postCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var postCategory = await _context.PostCategories.FirstOrDefaultAsync(m => m.PostCategoryId == id);
            if (postCategory == null) return NotFound();
            return View(postCategory);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postCategory = await _context.PostCategories.FindAsync(id);
            if (postCategory != null)
            {
                _context.PostCategories.Remove(postCategory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
