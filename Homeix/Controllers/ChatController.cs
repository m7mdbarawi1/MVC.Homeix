using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    public class ChatController : Controller
    {
        private readonly HOMEIXDbContext _context;

        public ChatController(HOMEIXDbContext context)
        {
            _context = context;
        }

        // =========================
        // TEMP USER RESOLUTION
        // =========================
        private async Task<int> GetCurrentUserIdAsync(int? forcedUserId)
        {
            if (forcedUserId.HasValue)
            {
                bool exists = await _context.Users.AnyAsync(u => u.UserId == forcedUserId);
                if (exists) return forcedUserId.Value;
            }

            // fallback (safe)
            return await _context.Users
                .OrderBy(u => u.UserId)
                .Select(u => u.UserId)
                .FirstAsync();
        }

        // =========================
        // GET: /Chat?userId=5&me=2
        // =========================
        public async Task<IActionResult> Index(int? userId, int? me)
        {
            int currentUserId = await GetCurrentUserIdAsync(me);

            // Sidebar users
            var users = await _context.Users
                .Where(u => u.UserId != currentUserId)
                .OrderBy(u => u.FullName)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.CurrentUserId = currentUserId;

            if (userId == null)
            {
                ViewBag.SelectedUser = null;
                ViewBag.Messages = new List<Message>();
                ViewBag.ConversationId = null;
                return View();
            }

            var selectedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (selectedUser == null)
                return NotFound();

            // Find or create conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == currentUserId && c.User2Id == userId) ||
                    (c.User1Id == userId && c.User2Id == currentUserId)
                );

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = currentUserId,
                    User2Id = userId.Value
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversation.ConversationId)
                .OrderBy(m => m.SentAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.SelectedUser = selectedUser;
            ViewBag.Messages = messages;
            ViewBag.ConversationId = conversation.ConversationId;

            return View();
        }

        // =========================
        // POST: /Chat/Send
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(
            int conversationId,
            string messageText,
            int me)
        {
            int currentUserId = await GetCurrentUserIdAsync(me);

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(messageText))
            {
                int other =
                    conversation.User1Id == currentUserId
                        ? conversation.User2Id
                        : conversation.User1Id;

                return RedirectToAction(nameof(Index), new { userId = other, me });
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = currentUserId,
                MessageText = messageText
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            int otherUserId =
                conversation.User1Id == currentUserId
                    ? conversation.User2Id
                    : conversation.User1Id;

            return RedirectToAction(nameof(Index), new { userId = otherUserId, me });
        }
    }
}
