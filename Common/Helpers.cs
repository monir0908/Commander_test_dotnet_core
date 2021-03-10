using System;
using System.Linq;
using Commander.Models;

namespace Commander.Common{
    public class Helpers
    {
        
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
                ConferenceDuration = actualCallDuration,
                ParticipantJoined = count,
            };
        }



    }
}