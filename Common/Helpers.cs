using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commander.Models;

namespace Commander.Common{
    public class Helpers
    {

        public static string GlobalProperty = "Class";
        
        private readonly ApplicationDbContext _context;  
 
        
        public Helpers(ApplicationDbContext context)
        {
            this._context = context;
        }

        public string GenerateRoomNumber()
        {
            string lastRoomNumber = _context.Conference.OrderByDescending(x => x.Id).Select(x => x.RoomId).FirstOrDefault();
            if (lastRoomNumber == null)
            {
                return "Room-101";
            }
            var splitItems = lastRoomNumber.Split(new string[] { "Room-" }, StringSplitOptions.None);
            int lastRoomNumberInt = Convert.ToInt32(splitItems[1]);
            int newlastRoomNumberInt = lastRoomNumberInt + 1;
            var newlastRoomNumber = "Room-" + Convert.ToString(newlastRoomNumberInt);

            // int milliseconds = 5000;
            // Thread.Sleep(milliseconds);
            // await Task.Delay(5000);

            return newlastRoomNumber;
        }
        

        public string GenerateVirtualClassRoomNumber()
        {
            string lastRoomNumber = _context.VClass.OrderByDescending(x => x.Id).Select(x => x.RoomId).FirstOrDefault();
            if (lastRoomNumber == null)
            {
                return "Room-101";
            }
            var splitItems = lastRoomNumber.Split(new string[] { "Room-" }, StringSplitOptions.None);
            int lastRoomNumberInt = Convert.ToInt32(splitItems[1]);
            int newlastRoomNumberInt = lastRoomNumberInt + 1;
            var newlastRoomNumber = "Room-" + Convert.ToString(newlastRoomNumberInt);

            // int milliseconds = 5000;
            // Thread.Sleep(milliseconds);
            // await Task.Delay(5000);

            return newlastRoomNumber;
        }


        public CallDurationHistory GetEffectiveCallDurationBetweenHostAndParticipant(long confId)
        {
            var confHistoryObj = _context.ConferenceHistory
            .Where(cs => cs.ConferenceId == confId)
            .Select(cs => new{
                cs.ConferenceId,
                cs.HostId,
                cs.ParticipantId,
                cs.RoomId,
                cs.JoineDateTime,
                cs.LeaveDateTime
            }).ToList();

            var hostObj = confHistoryObj
            .Where(cs => cs.HostId !=null)
            .Select(cs => cs)
            .FirstOrDefault();

            var participantObjs = confHistoryObj
            .Where(cs => cs.ParticipantId !=null)
            .Select(cs => cs)
            .ToList();

            //(StartDate1 <= EndDate2) and (StartDate2 <= EndDate1)
            int count = 0;
            TimeSpan? diff;
            TimeSpan? actualCallDuration = new TimeSpan(0,0,0);

            foreach(var i in participantObjs){

                count = count + 1;

                var particpantStartDate = i.JoineDateTime;
                var particpantEndDate = i.LeaveDateTime;


                //Note: in case, participant joins in earlier than host and leaves later than host.
                if(hostObj.JoineDateTime> particpantStartDate){
                    particpantStartDate = hostObj.JoineDateTime;
                }
                if(hostObj.LeaveDateTime< particpantEndDate){
                    particpantEndDate = hostObj.LeaveDateTime;
                }




                
                if((hostObj.JoineDateTime < particpantEndDate) && (particpantStartDate <hostObj.LeaveDateTime)){
                    //Console.WriteLine("-----------------Participant start :" + particpantStartDate + " " + "to Participant end :" + particpantEndDate + " range is inside calling Host call duration");
                    diff = particpantEndDate - particpantStartDate;
                    //Console.WriteLine("-------------------------Participant start :" + particpantStartDate);
                    //Console.WriteLine("-------------------------Participant end :" + particpantEndDate);
                    //Console.WriteLine("-----------------Diferrence :" + diff);
                    actualCallDuration = actualCallDuration + diff;
                }

            }

            //Console.WriteLine("How many times participant joined the conference? : " + count);
            //Console.WriteLine("Actual Call Duration : " + actualCallDuration);


            return new CallDurationHistory(){
                ActualCallDuration = actualCallDuration,
                ParticipantJoined = count,
            };
        }

        public CallDurationHistory GetActualCallDurationBetweenHostAndParticipant(long confId)
        {

            //Getting  VClassDetail objects
            var confHistoryObj = _context.VClassDetail
            .Where(cs => cs.VClassId == confId)
            .Select(cs => new{
                cs.VClassId,
                cs.HostId,
                cs.ParticipantId,
                cs.RoomId,
                cs.JoinTime,
                cs.LeaveTime
            }).ToList();

            // Separating Host obj
            var hostObj = confHistoryObj
            .Where(cs => cs.HostId !=null)
            .Select(cs => cs)
            .FirstOrDefault();


            // Separating Participants 
            var participantObjs = confHistoryObj
            .Where(cs => cs.ParticipantId !=null)
            .Select(cs => cs)
            .OrderBy(c => c.JoinTime).ThenBy(c => c.LeaveTime) // this is the most important !!! We need all 'JoinTime' & 'LeaveTime' in an ascending order
            .ToList();

            if(participantObjs.Count<1)
            return new CallDurationHistory(){
                HostCallDuration = hostObj.LeaveTime - hostObj.JoinTime,
                ParticipantsCallDuration = new TimeSpan(0,0,0),
                EmptySlotDuration = new TimeSpan(0,0,0),
                ActualCallDuration = new TimeSpan(0,0,0),
                ParticipantJoined = 0,
                UniqueParticipantCounts = 0,
            }; 

            // If we want to know how many unique participants were in the conference
            var uniqueParticipants = participantObjs
            .GroupBy(n => n.ParticipantId)
            .Select(n => n.Key);

            


           


            //How long Host was in the call?
            TimeSpan? hostCallDuration = hostObj.LeaveTime - hostObj.JoinTime;


            //How long participants were in the call? (Note: it might have multiple emptyslots)
            DateTime participantFirstEntryTime = participantObjs[0].JoinTime??new DateTime();
            DateTime participantLastLeftTime = participantObjs[participantObjs.Count()-1].LeaveTime??new DateTime();
            TimeSpan participantsCallDuration = participantLastLeftTime - participantFirstEntryTime; 

            
            // What would be actual call duration?
            // If we deduct 'emptySlots' from 'participantsCallDuration', we can find 'actualCallDuration' 
            TimeSpan actualCallDuration;
            

            
            //Getting first 'JoinTime' and first 'LeaveTime' which will be checked against 'participantObjs' all-start-end-datetimes
            DateTime tempStart = participantObjs[0].JoinTime??new DateTime();
            DateTime tempEnd = participantObjs[0].LeaveTime??new DateTime();

            TimeSpan emptySlotDuration; 
            TimeSpan totalEmptySlotDuration = new TimeSpan(0,0,0);

            foreach (var item in participantObjs)
            {
                DateTime loopStart = item.JoinTime?? new DateTime();
                DateTime loopEnd = item.LeaveTime?? new DateTime();


                // Checking if 'tempStart--tempEnd' range intersects with 'loopStart---loopEnd' range
                if((tempStart <= loopEnd) && (loopStart <=tempEnd))
                {
                    Console.WriteLine("=====================================================================");
                    Console.WriteLine("# '" + tempStart + "' ---- '" + tempEnd + "' intersects with '" + loopStart + "' ----- '" + loopEnd + "'");
                    Console.WriteLine();
                    


                    // When 'tempStart -- tempEnd' range intersects with 'loopStart -- loopEnd' range,
                    // we then need to create a new range where tempStart will remain same but 'tempEnd' need
                    // to be replaced with 'loopEnd'.  
                    
                    tempEnd = loopEnd;

                    Console.WriteLine("Next range would be : " + tempStart + " - " + tempEnd);
                    Console.WriteLine();
                    Console.WriteLine();


                }
                else
                {
                    // When range does not intersects with next range, it means
                    // there would be empty slot where no participants were available 
                    // in the conference.

                    // emptySlotDuration = second range start (i.e. loopStart) - first range end (i.e. tempEnd)

                    Console.WriteLine("---------------------------------------------------------------------");
                    Console.WriteLine("Empty slot found !!!");
                    Console.WriteLine("Empty slot is : " + tempEnd + " - " + loopStart); 

                    emptySlotDuration = loopStart - tempEnd;
                    Console.WriteLine("Empty Slot duration is : " + emptySlotDuration );
                    Console.WriteLine();
                    Console.WriteLine();

                    // Since empty slot is found, we can set next range as the follwoing                                        
                    tempStart = loopStart;
                    tempEnd = loopEnd;

                    
                    // Since 'empty slot' is found, we can keep on adding 'empty slot' duration
                    // to get total-empty-slot-duration 
                    totalEmptySlotDuration = totalEmptySlotDuration + emptySlotDuration;
                    

                }

                
            }

            // Finding out actual call duration 
                actualCallDuration = participantsCallDuration - totalEmptySlotDuration;

                Console.WriteLine("----------FINAL RESULT-----------------");
                Console.WriteLine("Total Empty Slot Duration is : " + totalEmptySlotDuration );
                Console.WriteLine("Actual Call Duration is : " + actualCallDuration );
                Console.WriteLine("---------------------------------------");   


            return new CallDurationHistory(){
                HostCallDuration = hostCallDuration,
                ParticipantsCallDuration = participantsCallDuration,
                EmptySlotDuration = totalEmptySlotDuration,
                ActualCallDuration = actualCallDuration,
                ParticipantJoined = participantObjs.Count(),
                UniqueParticipantCounts = uniqueParticipants.Count(),
            };
        }

        // public void TakeActionsWhenCallGetDisconnected(string connectionId){
            

        //     //Console.WriteLine("NOTE:====================================================JUST DISCONNECTED SOCKETID : " + connectionId);

        //     ConferenceHistory confHistoryObj = _context.ConferenceHistory.Where(x =>x.ConnectionId == connectionId ).Select(x => x).OrderByDescending(x=> x.Id).FirstOrDefault();

            

        //     if(confHistoryObj == null){

        //         //Console.WriteLine("NOTE: NO CONFERENCE FOUND WITH THIS SOCKET ID :" + connectionId);
        //         return base.OnDisconnectedAsync(exception);

        //     }

        //     // Note: if confHistoryObj is not null then we can find who got disconnected through its connectionid (i.e. socketId)
        //     else{                

        //         //Scenario 01: Host got disconnected
        //         if(confHistoryObj.HostId !=null){                    

        //             Console.WriteLine("######################################################################################");    
        //             Console.WriteLine("JUST DISCONNECTED SOCKETID : " + connectionId);
        //             Console.WriteLine("Host '" + confHistoryObj.HostId + "' got disconnected");


        //             //NOTE: 'LeaveDateTime' property is null means, an on-going meeting exits and need to update property.
        //             if(confHistoryObj.LeaveDateTime == null){

        //                 //Step 01: Update 'LeaveDateTime' property on ConferenceHistory Table if 'LeaveDateTime' property is empty
        //                 confHistoryObj.LeaveDateTime = DateTime.UtcNow;
        //                 _context.SaveChanges();
        //                 Console.WriteLine("1. 'LeaveDateTime' updated for Host.");


        //                 //Step 02: Update 'Status' property on Conference Table
        //                 Conference confObj = _context.Conference.Where(co => co.Id == confHistoryObj.ConferenceId).Select(x=>x).FirstOrDefault();
        //                 confObj.Status = "Closed";
        //                 confObj.HasJoinedByHost = false;
        //                 confObj.HasJoinedByParticipant = false;
        //                 _context.SaveChanges();


        //                 Console.WriteLine("3. Conference status changed to 'Closed' for entire conference");
        //                 Console.WriteLine("4. Now, Host needs to end participant's session too. Finding partner's id.. .. ..");
        //                 Console.WriteLine("5. Host's partner/participant Id is : "+ confObj.ParticipantId);



        //                 //Step 01: This time Update PARTICIPANT's 'LeaveDateTime' property on ConferenceHistory Table to end participant session too.
        //                 ConferenceHistory confHistoryObj2 = _context.ConferenceHistory
        //                 .Where(co => co.ConferenceId == confObj.Id && co.ParticipantId == confObj.ParticipantId)
        //                 .Select(x=>x).OrderByDescending(x=> x.Id)
        //                 .FirstOrDefault();
                        
        //                 if(confHistoryObj2.LeaveDateTime ==null){
        //                     confHistoryObj2.LeaveDateTime = DateTime.UtcNow;
        //                     _context.SaveChanges();
        //                     Console.WriteLine("6. Now, 'LeaveDateTime' is updated for 'PARTICIAPNT' which is currently empty."); 
        //                 }
        //                 else{
        //                     Console.WriteLine("6. 'LeaveDateTime' is NOT empty for participant; So 'LeaveDateTime' not updated again.");
        //                 }
                        
        //                 // Now, signalR comes into play
        //                 Clients.All.SendAsync("BrowserRefreshedOrInternetInteruption", confObj.ParticipantId);
        //                 Console.WriteLine("7. Finally, alerting participant about conference termination through signalR, participant id is : " + confObj.ParticipantId);

        //             }
                    
        //             else
        //             {
        //                 Console.WriteLine("Host 'LeaveDateTime' property is already set. No action taken!");
        //             }
        //         }

        //         //Scenario 02: Participant got disconnected
        //         else if(confHistoryObj.ParticipantId != null){                    

        //             Console.WriteLine("######################################################################################");
        //             Console.WriteLine("DISCONNECTED SOCKETID : " + connectionId);
        //             Console.WriteLine("Participant '" + confHistoryObj.ParticipantId + "' got disconnected");


                    
        //             //Step 01: Update 'LeaveDateTime' property on ConferenceHistory Table if 'LeaveDateTime' property is empty
        //             if(confHistoryObj.LeaveDateTime == null){                
        //                 confHistoryObj.LeaveDateTime = DateTime.UtcNow;
        //                 _context.SaveChanges();
        //                 Console.WriteLine("I am participant and my 'LeaveDateTime' is empty, so it is updated now.");


        //                 Conference confObj = _context.Conference.Where(co => co.Id == confHistoryObj.ConferenceId).Select(x=>x).FirstOrDefault();
        //                 confObj.HasJoinedByParticipant = false;
        //                 _context.SaveChanges();                
        //             }
        //             else
        //             {                
        //                 Console.WriteLine("I am participant and my 'LeaveDateTime' property is already set. No action taken!");
        //             }
                    
        //         }
        //     }
        // }

        


    }
}