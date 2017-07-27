using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using System.Linq;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        public void SendChatMessage(string senderName, string receiverName, string message)
        {
            _connections.Add(senderName, Context.ConnectionId);

            if (string.IsNullOrEmpty(receiverName))
                Clients.All.broadcastMessage(senderName, message);
            else
            {
                foreach (var connectionId in _connections.GetConnections(receiverName))
                {
                    Clients.Client(connectionId).addChatMessage(senderName, message);
                }
            }
            
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            _connections.RemoveByConnectionId(Context.ConnectionId);
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}