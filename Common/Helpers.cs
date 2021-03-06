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



    }
}