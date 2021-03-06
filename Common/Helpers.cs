using System;
using System.Linq;
using Commander.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Commander.Common{
    public static class Helpers
    {
        private static readonly IConfiguration _configuration;
        private static string _connectionString;
        private static DbContextOptionsBuilder<ApplicationDbContext> _optionsBuilder;

        private static readonly ApplicationDbContext _context;



        static Helpers()
        {
            _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            _connectionString = _configuration.GetConnectionString("Cn");
            _optionsBuilder.UseSqlServer(_connectionString);
            _context = new ApplicationDbContext(_optionsBuilder.Options);
        }

        public static string GenerateRoomNumber()
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