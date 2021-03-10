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

            //Console.WriteLine("NOTE:====================================================JUST DISCONNECTED SOCKETID : " + connectionId);

            ConferenceHistory confHistoryObj = _context.ConferenceHistory.Where(x =>x.ConnectionId == connectionId ).Select(x => x).OrderByDescending(x=> x.Id).FirstOrDefault();

            

            if(confHistoryObj == null){

                //Console.WriteLine("NOTE: NO CONFERENCE FOUND WITH THIS SOCKET ID :" + connectionId);
                return base.OnDisconnectedAsync(exception);

            }

            // Note: if confHistoryObj is not null then we can find who got disconnected through its connectionid (i.e. socketId)
            else{                

                //Scenario 01: Host got disconnected
                if(confHistoryObj.HostId !=null){                    

                    Console.WriteLine("######################################################################################");    
                    Console.WriteLine("JUST DISCONNECTED SOCKETID : " + connectionId);
                    Console.WriteLine("Host '" + confHistoryObj.HostId + "' got disconnected");


                    //NOTE: 'LeaveDateTime' property is null means, an on-going meeting exits and need to update property.
                    if(confHistoryObj.LeaveDateTime == null){

                        //Step 01: Update 'LeaveDateTime' property on ConferenceHistory Table if 'LeaveDateTime' property is empty
                        confHistoryObj.LeaveDateTime = DateTime.UtcNow;
                        _context.SaveChanges();
                        Console.WriteLine("1. 'LeaveDateTime' updated for Host.");


                        //Step 02: Update 'Status' property on Conference Table
                        Conference confObj = _context.Conference.Where(co => co.Id == confHistoryObj.ConferenceId).Select(x=>x).FirstOrDefault();
                        confObj.Status = "Closed";
                        _context.SaveChanges();


                        Console.WriteLine("3. Conference status changed to 'Closed' for entire conference");
                        Console.WriteLine("4. Now, Host needs to end participant's session too. Finding partner's id.. .. ..");
                        Console.WriteLine("5. Host's partner/participant Id is : "+ confObj.ParticipantId);



                        //Step 01: This time Update PARTICIPANT's 'LeaveDateTime' property on ConferenceHistory Table to end participant session too.
                        ConferenceHistory confHistoryObj2 = _context.ConferenceHistory
                        .Where(co => co.ConferenceId == confObj.Id && co.ParticipantId == confObj.ParticipantId)
                        .Select(x=>x).OrderByDescending(x=> x.Id)
                        .FirstOrDefault();
                        
                        if(confHistoryObj2.LeaveDateTime ==null){
                            confHistoryObj2.LeaveDateTime = DateTime.UtcNow;
                            _context.SaveChanges();
                            Console.WriteLine("6. Now, 'LeaveDateTime' is updated for 'PARTICIAPNT' which is currently empty."); 
                        }
                        else{
                            Console.WriteLine("6. 'LeaveDateTime' is NOT empty for participant; So 'LeaveDateTime' not updated again.");
                        }
                        
                        // Now, signalR comes into play
                        Clients.All.SendAsync("BrowserRefreshedOrInternetInteruption", confObj.ParticipantId);
                        Console.WriteLine("7. Finally, alerting participant about conference termination through signalR, participant id is : " + confObj.ParticipantId);

                    }
                    
                    else
                    {
                        Console.WriteLine("Host 'LeaveDateTime' property is already set. No action taken!");
                    }
                }

                //Scenario 02: Participant got disconnected
                else if(confHistoryObj.ParticipantId != null){                    

                    Console.WriteLine("######################################################################################");
                    Console.WriteLine("DISCONNECTED SOCKETID : " + connectionId);
                    Console.WriteLine("Participant '" + confHistoryObj.ParticipantId + "' got disconnected");


                    
                    //Step 01: Update 'LeaveDateTime' property on ConferenceHistory Table if 'LeaveDateTime' property is empty
                    if(confHistoryObj.LeaveDateTime == null){                
                        confHistoryObj.LeaveDateTime = DateTime.UtcNow;
                        _context.SaveChanges();
                        Console.WriteLine("I am participant and my 'LeaveDateTime' is empty, so it is updated now.");                
                    }
                    else
                    {                
                        Console.WriteLine("I am participant and my 'LeaveDateTime' property is already set. No action taken!");
                    }
                    
                }
            }

            return base.OnDisconnectedAsync(exception);
            




            
        }
    }
}
