using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Models;
using Homeix.Data;

namespace Homeix.Controllers
{
    public class PostMediumsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public PostMediumsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // GET: PostMediums
        public async Task<IActionResult> Index()
        {
            return View(await _context.PostMedia.ToListAsync());
        }

        // GET: PostMediums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postMedium = await _context.PostMedia
                .FirstOrDefaultAsync(m => m.MediaId == id);
            if (postMedium == null)
            {
                return NotFound();
            }

            return View(postMedium);
        }

        // GET: PostMediums/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PostMediums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MediaId,PostType,PostId,MediaPath,UploadedAt")] PostMedium postMedium)
        {
            if (ModelState.IsValid)
            {
                _context.Add(postMedium);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(postMedium);
        }

        // GET: PostMediums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium == null)
            {
                return NotFound();
            }
            return View(postMedium);
        }

        // POST: PostMediums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MediaId,PostType,PostId,MediaPath,UploadedAt")] PostMedium postMedium)
        {
            if (id != postMedium.MediaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postMedium);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostMediumExists(postMedium.MediaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(postMedium);
        }

        // GET: PostMediums/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postMedium = await _context.PostMedia
                .FirstOrDefaultAsync(m => m.MediaId == id);
            if (postMedium == null)
            {
                return NotFound();
            }

            return View(postMedium);
        }

        // POST: PostMediums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var postMedium = await _context.PostMedia.FindAsync(id);
            if (postMedium != null)
            {
                _context.PostMedia.Remove(postMedium);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostMediumExists(int id)
        {
            return _context.PostMedia.Any(e => e.MediaId == id);
        }
    }
}
