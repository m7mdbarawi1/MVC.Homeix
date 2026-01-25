using Homeix.Data;
using Homeix.Models;
using Homeix.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            {
                throw new InvalidOperationException("Authenticated user does not have a valid UserId claim.");
            }
            return userId;
        }
        public async Task<IActionResult> Index(int? userId)
        {
            int currentUserId = GetCurrentUserId();
            var users = await _context.Users.Where(u => u.UserId != currentUserId).OrderBy(u => u.FullName).AsNoTracking().ToListAsync();
            ViewBag.Users = users;
            ViewBag.CurrentUserId = currentUserId;
            if (userId == null)
            {
                ViewBag.SelectedUser = null;
                ViewBag.Messages = new List<Message>();
                ViewBag.ConversationId = null;
                return View();
            }
            var selectedUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId.Value);
            if (selectedUser == null) return NotFound();
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => (c.User1Id == currentUserId && c.User2Id == userId) || (c.User1Id == userId && c.User2Id == currentUserId));
            if (conversation == null)
            {
                conversation = new Conversation {User1Id = currentUserId, User2Id = userId.Value};
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }
            var messages = await _context.Messages.Where(m => m.ConversationId == conversation.ConversationId).OrderBy(m => m.SentAt).AsNoTracking().ToListAsync();
            ViewBag.SelectedUser = selectedUser;
            ViewBag.Messages = messages;
            ViewBag.ConversationId = conversation.ConversationId;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int conversationId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText)) return RedirectToAction(nameof(Index));
            int currentUserId = GetCurrentUserId();
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return NotFound();
            var message = new Message {ConversationId = conversationId, SenderUserId = currentUserId, MessageText = messageText, SentAt = DateTime.UtcNow};
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            int otherUserId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;
            await _hubContext.Clients.Groups($"user-{currentUserId}", $"user-{otherUserId}").SendAsync("ReceiveMessage", new{senderId = currentUserId, text = messageText, sentAt = message.SentAt.ToString("HH:mm")});
            return RedirectToAction(nameof(Index), new { userId = otherUserId });
        }
    }
}
