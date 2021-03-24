using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commander.Models;
using Microsoft.EntityFrameworkCore;
using Commander.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using Itenso.TimePeriod;

namespace Commander.Services{

    

    public class UserServices : IUserServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SignalHub> _notificationHubContext;

        public UserServices(ApplicationDbContext context, IHubContext<SignalHub> hubContext)
        {
            this._context = context;
            this._notificationHubContext = hubContext;
        }

        public async Task<object> GetUserList(int size, int pageNumber)
        {
            try
            {
                var data = await _context.Users.OrderByDescending(x => x.Id).Select(x => new 
                { 
                    x.Id, 
                    x.FirstName, 
                    x.LastName, 
                    x.Email, 
                    x.PhoneNumber,
                    x.UserName,
                    x.UserType,
                })
                .Skip(pageNumber * size)
                .Take(size)
                .ToListAsync();

                var count = await _context.Users.CountAsync();

                return new
                {
                    Success = true,
                    Records = data,
                    Total = count
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }


        
        
        
    }
}


