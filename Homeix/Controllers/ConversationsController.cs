using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;

namespace Homeix.Controllers
{
    public class ConversationsController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public ConversationsController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // ========================
        // GET: Conversations
        // ========================
        public async Task<IActionResult> Index()
        {
            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .ToListAsync();

            return View(conversations);
        }

        // ========================
        // ✅ DOWNLOAD REPORT (CSV)
        // ========================
        public async Task<IActionResult> DownloadReport()
        {
            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ConversationId,CreatedAt,User1Id,User2Id");

            foreach (var c in conversations)
            {
                sb.AppendLine(
                    $"{c.ConversationId}," +
                    $"{c.CreatedAt:yyyy-MM-dd HH:mm}," +
                    $"{c.User1Id}," +
                    $"{c.User2Id}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "ConversationsReport.csv");
        }

        // ========================
        // GET: Conversations/Details/5
        // ========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var conversation = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conversation == null)
                return NotFound();

            return View(conversation);
        }

        // ========================
        // GET: Conversations/Create
        // ========================
        public IActionResult Create()
        {
            LoadUsersDropdowns();
            return View();
        }

        // ========================
        // POST: Conversations/Create
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("User1Id,User2Id")]
            Conversation conversation)
        {
            if (!ModelState.IsValid)
            {
                LoadUsersDropdowns(conversation.User1Id, conversation.User2Id);
                return View(conversation);
            }

            conversation.CreatedAt = DateTime.Now;

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Conversations/Edit/5
        // ========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation == null)
                return NotFound();

            LoadUsersDropdowns(conversation.User1Id, conversation.User2Id);
            return View(conversation);
        }

        // ========================
        // POST: Conversations/Edit/5
        // ========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ConversationId,User1Id,User2Id")]
            Conversation conversation)
        {
            if (id != conversation.ConversationId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                LoadUsersDropdowns(conversation.User1Id, conversation.User2Id);
                return View(conversation);
            }

            var existing = await _context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (existing == null)
                return NotFound();

            conversation.CreatedAt = existing.CreatedAt;

            _context.Update(conversation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // GET: Conversations/Delete/5
        // ========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var conversation = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conversation == null)
                return NotFound();

            return View(conversation);
        }

        // ========================
        // POST: Conversations/Delete/5
        // ========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conversation = await _context.Conversations.FindAsync(id);
            if (conversation != null)
            {
                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================
        // Helpers
        // ========================
        private void LoadUsersDropdowns(int? user1Id = null, int? user2Id = null)
        {
            ViewData["User1Id"] =
                new SelectList(_context.Users, "UserId", "UserId", user1Id);

            ViewData["User2Id"] =
                new SelectList(_context.Users, "UserId", "UserId", user2Id);
        }
    }
}
