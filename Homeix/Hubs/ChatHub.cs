using Homeix.Data;
using Homeix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly HOMEIXDbContext _context;

        public ChatHub(HOMEIXDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdValue = Context.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var id))
                throw new HubException("Authenticated user does not have a valid UserId claim.");
            return id;
        }

        /* ================= CONNECTION ================= */

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // notify others (not self)
            await Clients.Others.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdValue = Context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrWhiteSpace(userIdValue) && int.TryParse(userIdValue, out var userId))
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /* ================= SEND MESSAGE ================= */
        // Signature MUST stay the same (client depends on it)
        public async Task SendMessage(int conversationId, int senderId, int receiverId, string message)
        {
            var currentUserId = GetCurrentUserId();

            // prevent spoofing
            if (senderId != currentUserId)
                throw new HubException("Invalid sender.");

            if (string.IsNullOrWhiteSpace(message))
                return;

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);

            if (conversation == null)
                throw new HubException("Conversation not found.");

            bool validPair =
                (conversation.User1Id == currentUserId && conversation.User2Id == receiverId) ||
                (conversation.User2Id == currentUserId && conversation.User1Id == receiverId);

            if (!validPair)
                throw new HubException("Not allowed for this conversation.");

            var msg = new Message
            {
                ConversationId = conversationId,
                SenderUserId = currentUserId,
                MessageText = message.Trim(),
                SentAt = DateTime.Now
            };

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            await Clients.Groups($"user-{currentUserId}", $"user-{receiverId}")
                .SendAsync("ReceiveMessage", new
                {
                    conversationId,
                    senderId = currentUserId,
                    receiverId,
                    message = msg.MessageText,
                    sentAt = msg.SentAt.ToString("HH:mm")
                });
        }

        /* ================= TYPING ================= */
        public async Task Typing(int receiverId, int conversationId = 0)
        {
            var senderId = GetCurrentUserId();

            if (receiverId <= 0 || conversationId <= 0)
                return;

            await Clients.Group($"user-{receiverId}")
                .SendAsync("UserTyping", new
                {
                    conversationId,
                    senderId
                });
        }

        /* ================= SEEN ================= */
        public async Task MessageSeen(int senderId, int conversationId = 0)
        {
            var viewerId = GetCurrentUserId();

            if (senderId <= 0 || conversationId <= 0)
                return;

            // notify ONLY the sender that their message was seen
            await Clients.Group($"user-{senderId}")
                .SendAsync("MessageSeen", new
                {
                    conversationId,
                    viewerId
                });
        }
    }
}
