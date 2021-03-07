using Commander.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Commander
{
    public class SignalHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public SignalHub(ApplicationDbContext context)
        {
            this._context = context;
        }
        public void GetDataFromClient(string userId, string connectionId)
        {
            
            Clients.Client(connectionId).SendAsync("clientMethodName", $"Updated userid {userId}");
        }
        // public void ABCMethodCallableFromClient(string hostId, string participantId, string roomId, string connectionId)
        // {
        //     Clients.Client(connectionId).SendAsync("XYZMethodTobeListenedTo", $"Updated HOSTID {hostId}");
        // }

        public void ABCMethodCallableFromClient(string hostId, string connectionId)
        {
            Clients.Client(connectionId).SendAsync("XYZMethodTobeListenedTo", $"Updated HOSTID is :  {hostId}");
        }

        // public async Task TaskCompleted(int id)
        // {
        //     await Clients.All.SendAsync("Completed", id);
        // }










        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Clients.Client(connectionId).SendAsync("WelcomeMethodName", connectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            return base.OnDisconnectedAsync(exception);
        }
    }
}
