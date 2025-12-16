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
    public class MessagesController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public MessagesController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Messages
        // ========================
        public async Task<IActionResult> Index()
        {
            var messages = await _context.Messages
                .Include(m => m.Conversation)
                .Include(m => m.SenderUser)
                .ToListAsync();

            return View(messages);
        }

        // ========================
        // GET: Messages/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var message = await _context.Messages
                .Include(m => m.Conversation)
                .Include(m => m.SenderUser)
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null)
                return NotFound();

            return View(message);
        }

        // ========================
        // GET: Messages/Create
        // ========================
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }

        // ========================
        // POST: Messages/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "ConversationId," +
                "SenderUserId," +
                "MessageText"
            )]
            Message message)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(message);
                return View(message);
            }

            // =========================
            // System-managed fields
            // =========================
            message.SentAt = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Messages/Edit
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            ReloadDropdowns(message);
            return View(message);
        }

        // ========================
        // POST: Messages/Edit
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "MessageId," +
                "ConversationId," +
                "SenderUserId," +
                "MessageText"
            )]
            Message message)
        {
            if (id != message.MessageId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ReloadDropdowns(message);
                return View(message);
            }

            var existing = await _context.Messages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (existing == null)
                return NotFound();

            // =========================
            // Preserve system fields
            // =========================
            message.SentAt = existing.SentAt;

            _context.Update(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Messages/Delete
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var message = await _context.Messages
                .Include(m => m.Conversation)
                .Include(m => m.SenderUser)
                .FirstOrDefaultAsync(m => m.MessageId == id);

            if (message == null)
                return NotFound();

            return View(message);
        }

        // ========================
        // POST: Messages/Delete
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void ReloadDropdowns(Message? message = null)
        {
            ViewData["ConversationId"] = new SelectList(
                _context.Conversations,
                "ConversationId",
                "ConversationId",
                message?.ConversationId
            );

            ViewData["SenderUserId"] = new SelectList(
                _context.Users,
                "UserId",
                "UserId",
                message?.SenderUserId
            );
        }
    }
}
