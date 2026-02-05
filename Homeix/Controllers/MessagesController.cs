using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Homeix.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Homeix.Controllers
{
    public class MessagesController : Controller
    {
        private readonly HOMEIXDbContext _context;
        public MessagesController(HOMEIXDbContext context) { _context = context;}
        
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var messages = await _context.Messages.Include(m => m.Conversation).Include(m => m.SenderUser).ToListAsync();
            return View(messages);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var messages = await _context.Messages.Include(m => m.Conversation).Include(m => m.SenderUser).OrderByDescending(m => m.SentAt).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("MessageId,MessageText,SentAt,ConversationId,SenderUserId");
            foreach (var m in messages)
            {
                sb.AppendLine($"{m.MessageId}," + $"\"{m.MessageText?.Replace("\"", "\"\"")}\"," + $"{m.SentAt:yyyy-MM-dd HH:mm}," + $"{m.ConversationId}," + $"{m.SenderUserId}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "MessagesReport.csv");
        }
        
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var message = await _context.Messages.Include(m => m.Conversation).Include(m => m.SenderUser).FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null) return NotFound();
            return View(message);
        }
        
        [Authorize]
        public IActionResult Create()
        {
            ReloadDropdowns();
            return View();
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ConversationId,SenderUserId,MessageText")] Message message)
        {
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(message);
                return View(message);
            }
            message.SentAt = DateTime.Now;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return NotFound();
            ReloadDropdowns(message);
            return View(message);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( int id, [Bind("MessageId,ConversationId,SenderUserId,MessageText")] Message message)
        {
            if (id != message.MessageId) return NotFound();
            if (!ModelState.IsValid)
            {
                ReloadDropdowns(message);
                return View(message);
            }
            var existing = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.MessageId == id);
            if (existing == null) return NotFound();
            message.SentAt = existing.SentAt;
            _context.Update(message);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var message = await _context.Messages.Include(m => m.Conversation).Include(m => m.SenderUser).FirstOrDefaultAsync(m => m.MessageId == id);
            if (message == null) return NotFound();
            return View(message);
        }

        [Authorize]
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
        private void ReloadDropdowns(Message? message = null)
        {
            ViewData["ConversationId"] = new SelectList(_context.Conversations, "ConversationId", "ConversationId", message?.ConversationId);
            ViewData["SenderUserId"] = new SelectList(_context.Users, "UserId", "UserId", message?.SenderUserId);
        }
    }
}
