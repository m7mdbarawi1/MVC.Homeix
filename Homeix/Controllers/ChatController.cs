using Homeix.Data;
using Homeix.Hubs;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly HOMEIXDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(HOMEIXDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new InvalidOperationException("Authenticated user does not have a valid UserId claim.");
            return userId;
        }

        public async Task<IActionResult> Index(int? userId)
        {
            int currentUserId = GetCurrentUserId();

            // --- USERS LIST: ALL USERS, ordered by last message with current user ---
            var lastMessageByOtherUser = _context.Messages
                .Where(m => m.Conversation!.User1Id == currentUserId || m.Conversation.User2Id == currentUserId)
                .GroupBy(m => m.Conversation!.User1Id == currentUserId ? m.Conversation.User2Id : m.Conversation.User1Id)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessageAt = g.Max(x => (DateTime?)x.SentAt)
                });

            var users = await _context.Users
                .Where(u => u.UserId != currentUserId)
                .GroupJoin(
                    lastMessageByOtherUser,
                    u => u.UserId,
                    x => x.OtherUserId,
                    (u, g) => new { User = u, LastMessageAt = g.Select(y => y.LastMessageAt).FirstOrDefault() }
                )
                .OrderByDescending(x => x.LastMessageAt.HasValue)
                .ThenByDescending(x => x.LastMessageAt)
                .ThenBy(x => x.User.FullName)
                .Select(x => x.User)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.CurrentUserId = currentUserId;

            // --- AUTO OPEN LAST CONVERSATION (when userId is not provided) ---
            if (userId == null)
            {
                var lastChat = await _context.Conversations
                    .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                    .Select(c => new
                    {
                        OtherUserId = (c.User1Id == currentUserId) ? c.User2Id : c.User1Id,
                        LastMessageAt = _context.Messages
                            .Where(m => m.ConversationId == c.ConversationId)
                            .Max(m => (DateTime?)m.SentAt),
                        c.CreatedAt
                    })
                    .OrderByDescending(x => x.LastMessageAt.HasValue) // chats with messages first
                    .ThenByDescending(x => x.LastMessageAt)           // newest message
                    .ThenByDescending(x => x.CreatedAt)               // if no messages, newest conversation
                    .FirstOrDefaultAsync();

                userId = lastChat?.OtherUserId;
            }

            // No conversation found -> show empty state
            if (userId == null)
            {
                ViewBag.SelectedUser = null;
                ViewBag.Messages = new List<Message>();
                ViewBag.ConversationId = null;
                return View();
            }

            var selectedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (selectedUser == null)
                return NotFound();

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c =>
                    (c.User1Id == currentUserId && c.User2Id == userId) ||
                    (c.User1Id == userId && c.User2Id == currentUserId)
                );

            if (conversation == null)
            {
                conversation = new Conversation {User1Id = currentUserId, User2Id = userId.Value};
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

        // Keep this action working (in case you post messages by form somewhere)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int conversationId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return RedirectToAction(nameof(Index));
            int currentUserId = GetCurrentUserId();
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
                return NotFound();

            var message = new Message
            {
                ConversationId = conversationId,
                SenderUserId = currentUserId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            int otherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;

            await _hubContext.Clients.Groups($"user-{currentUserId}", $"user-{otherUserId}").SendAsync("ReceiveMessage", new{
                    senderId = currentUserId,
                    text = messageText,
                    sentAt = message.SentAt.ToString("HH:mm")
                });

            return RedirectToAction(nameof(Index), new { userId = otherUserId });
        }
    }
}