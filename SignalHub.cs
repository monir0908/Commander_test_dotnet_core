using Commander.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

        
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Clients.Client(connectionId).SendAsync("WelcomeMethodName", connectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            Clients.All.SendAsync("BrowserRefreshedOrInternetInteruption", connectionId);

            ConferenceHistory existingConfHistory = _context.ConferenceHistory.Where(x =>x.ConnectionId == connectionId ).Select(x => x).FirstOrDefault();

            if(existingConfHistory !=null && existingConfHistory.HostId !=null){
                existingConfHistory.LeaveDateTime = DateTime.UtcNow;
                _context.SaveChanges();


                Conference existingConf = _context.Conference.Where(co => co.Id == existingConfHistory.ConferenceId).Select(x=>x).FirstOrDefault();
                existingConf.Status = "Closed";
                _context.SaveChanges();


                ConferenceHistory existingConfHistory2 = _context.ConferenceHistory.Where(co => co.ConferenceId == existingConf.Id && co.ParticipantId == existingConf.ParticipantId).Select(x=>x).OrderByDescending(x=> x.Id).FirstOrDefault();
                
                if(existingConfHistory2.LeaveDateTime !=null){
                    existingConfHistory2.LeaveDateTime = DateTime.UtcNow;
                    _context.SaveChanges(); 
                }
                              



                // string status = "Closed";
                // _context.Database.ExecuteSqlRaw($"UPDATE [ConferenceHistory] SET Status = '{status}' WHERE ConferenceId = {existingConfHistory.ConferenceId}");

                // Now, signalR comes into play
                Clients.All.SendAsync("BrowserRefreshedOrInternetInteruption", existingConf.ParticipantId);
            }


            if(existingConfHistory !=null && existingConfHistory.ParticipantId !=null){
                existingConfHistory.LeaveDateTime = DateTime.UtcNow;
                _context.SaveChanges();
                
            }
            




            return base.OnDisconnectedAsync(exception);
        }
    }
}
