using Microsoft.AspNetCore.SignalR;

namespace Homeix.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId,$"user-{userId}");
                await Clients.Others.SendAsync("UserOnline", int.Parse(userId));
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userId)) {await Clients.Others.SendAsync("UserOffline", int.Parse(userId));}
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(int conversationId, int senderId, int receiverId, string message) {await Clients.Groups($"user-{senderId}", $"user-{receiverId}").SendAsync("ReceiveMessage", new {conversationId, senderId, message, sentAt = DateTime.Now.ToString("HH:mm")});}
        public async Task Typing(int receiverId) {await Clients.Group($"user-{receiverId}").SendAsync("UserTyping");}
        public async Task MessageSeen(int senderId){await Clients.Group($"user-{senderId}").SendAsync("MessageSeen");}
    }
}
