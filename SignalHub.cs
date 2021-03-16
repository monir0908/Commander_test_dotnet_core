using Commander.Common;
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

            // //Console.WriteLine("NOTE:====================================================JUST DISCONNECTED SOCKETID : " + connectionId);

            // VClassDetail vClassDetailObj = _context.VClassDetail.Where(x =>x.ConnectionId == connectionId ).Select(x => x).OrderByDescending(x=> x.Id).FirstOrDefault();

            

            // if(vClassDetailObj == null){

            //     //Console.WriteLine("NOTE: NO VClass FOUND WITH THIS SOCKET ID :" + connectionId);
            //     return base.OnDisconnectedAsync(exception);

            // }

            // // Note: if vClassObj is not null then we can find who got disconnected through its connectionid (i.e. socketId)
            // else{                

            //     //Scenario 01: Host got disconnected
            //     if(vClassDetailObj.HostId !=null){                    

            //         Console.WriteLine("######################################################################################");    
            //         Console.WriteLine("JUST DISCONNECTED SOCKETID : " + connectionId);
            //         Console.WriteLine("Host '" + vClassDetailObj.HostId + "' got disconnected");


            //         //NOTE: 'LeaveTime' property is null means, an on-going meeting exits and need to update property.
            //         if(vClassDetailObj.LeaveTime == null){

            //             //Step 01: Update 'LeaveTime' property on VClassHistory Table if 'LeaveTime' property is empty
            //             vClassDetailObj.LeaveTime = DateTime.UtcNow;
            //             _context.SaveChanges();
            //             Console.WriteLine("1. 'LeaveTime' updated for Host.");                        



            //             //Step 02: This time Update PARTICIPANT's 'LeaveTime' property on VClassHistory Table to end participant session too.
            //             var participantObjs = 
            //             _context.VClassDetail
            //             .Where(co => co.VClassId == vClassDetailObj.VClassId && co.RoomId == vClassDetailObj.RoomId && co.ParticipantId !=null)
            //             .Select(x=>x).OrderByDescending(x=> x.Id)
            //             .ToList();


            //             Console.WriteLine("2. 'LeaveTime' should be updated for Participants too.");

            //             if(participantObjs.Count()>0)
            //             {
            //                 foreach (var item in participantObjs)
            //                 {
            //                     if(item.LeaveTime ==null)
            //                     {
            //                         item.LeaveTime = DateTime.UtcNow;
            //                         _context.SaveChanges();
            //                         Console.WriteLine("---Now, 'LeaveTime' updated for 'PARTICIAPNT' " + item.ParticipantId); 
            //                     }
            //                     else
            //                     {
            //                         Console.WriteLine("---'LeaveTime' is NOT updated for participant" + item.ParticipantId + " Found 'LeaveTime already !");
            //                     }
                                
            //                     // Now, signalR comes into play
            //                     Clients.All.SendAsync("BrowserRefreshedOrInternetInteruption", item.ParticipantId);
            //                     Console.WriteLine("---Now, alerting participant about VClass termination through signalR, participant id is : " + item.ParticipantId);

            //                 }
            //             }
            //             else
            //             {
            //                 Console.WriteLine("---NOTE: No participants found for updating their 'LeaveTime'!");

            //             } 

                        


            //             //Step 02: Update 'Status' property on VClass Table
            //             VClass existingConf = _context.VClass.Where(x =>
            //             x.Id == vClassDetailObj.VClassId && 
            //             x.HostId == vClassDetailObj.HostId  && 
            //             x.RoomId == vClassDetailObj.RoomId)
            //             .Select(x => x)
            //             .OrderByDescending(x => x.Id)
            //             .FirstOrDefault();


            //             // Now, update 'VClass' table with call-details
            //             Helpers h = new Helpers(_context);
            //             var res = h.GetActualCallDurationBetweenHostAndParticipant(existingConf.Id);
            //             existingConf.Status = "Closed";

            //             existingConf.HostCallDuration = res.HostCallDuration;
            //             existingConf.ParticipantsCallDuration = res.ParticipantsCallDuration;
            //             existingConf.EmptySlotDuration = res.EmptySlotDuration;
            //             existingConf.ActualCallDuration = res.ActualCallDuration;
            //             existingConf.ParticipantJoined = res.ParticipantJoined;
            //             existingConf.UniqueParticipantCounts = res.UniqueParticipantCounts;
            //             _context.SaveChangesAsync();


            //             Console.WriteLine("3. VClass status changed to 'Closed' and call-necessary details updated in VClass Table");
            //         }
                    
            //         else
            //         {
            //             Console.WriteLine("Host 'LeaveTime' property is already set. No action taken!");
            //         }
            //     }

            //     //Scenario 02: Participant got disconnected
            //     else if(vClassDetailObj.ParticipantId != null){                    

            //         Console.WriteLine("######################################################################################");
            //         Console.WriteLine("DISCONNECTED SOCKETID : " + connectionId);
            //         Console.WriteLine("Participant '" + vClassDetailObj.ParticipantId + "' got disconnected");


                    
            //         //Step 01: Update 'LeaveTime' property on VClassHistory Table if 'LeaveTime' property is empty
            //         if(vClassDetailObj.LeaveTime == null)
            //         {                
            //             vClassDetailObj.LeaveTime = DateTime.UtcNow;
            //             _context.SaveChanges();
            //             Console.WriteLine("--- I am participant and my 'LeaveTime' is empty, so it is updated now.");
            //         }
            //         else
            //         {                
            //             Console.WriteLine("--- I am participant and my 'LeaveTime' property is already set. No action taken!");
            //         }
                    
            //     }
            // }

            return base.OnDisconnectedAsync(exception);
            




            
        }
    }
}
