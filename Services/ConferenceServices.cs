using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commander.Models;
using Microsoft.EntityFrameworkCore;
using Commander.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;

namespace Commander.Services{


    public class ConferenceServices : IConferenceServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SignalHub> _notificationHubContext;

        public ConferenceServices(ApplicationDbContext context, IHubContext<SignalHub> hubContext)
        {
            this._context = context;
            this._notificationHubContext = hubContext;
        }
        

        private bool IsSameRoomAleadyCreated(string roomId)
        {
            return _context.Conference.Any(x => x.RoomId == roomId);
        }


        private bool IsThereAnyOnGoingMeeting(string hostId)
        {
            return _context.Conference.Any(c => c.HostId == hostId && c.Status == "On-Going");
        }




        // Host Side

        public async Task<object> GetProjectListByHostId(string hostId)
        {
            try
            {

                
               IList<long> projectIds = _context.BatchHost.Where(x => x.HostId == hostId).Select(x => x.ProjectId).Distinct().ToArray();

               var query = _context.Project.Where(x => projectIds.Contains(x.Id)).AsQueryable();
               var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.ProjectName,
                }).ToListAsync();


               var count = projectIds.Count;

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }
        }

        public async Task<object> GetBatchListByProjectId(long pId)
        {


            try
            {
                var query = _context.Batch.Where(x => x.ProjectId == pId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.BatchName,

                    BatchParticipantList = _context.BatchHostParticipant.Where(p=> p.BatchId == x.Id)
                    .Select(p=> new
                    {
                        p.ParticipantId,
                        p.Participant.FirstName,
                        p.Participant.LastName
                    }).ToList(),
                }).ToListAsync();


                var count = await query.CountAsync();

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }

        }

        public async Task<object> GetParticipantListByBatchId(long batchId)
        {


            try
            {
                var query = _context.BatchHostParticipant.Where(x => x.BatchId == batchId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.ParticipantId,
                    x.Participant.FirstName,
                    x.Participant.LastName,
                    
                }).ToListAsync();


                var count = await query.CountAsync();

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }

        }

        public async Task<object> GetParticipantListByHostId(string hostId)
        {


            try
            {
                var query = _context.BatchHostParticipant.Where(x => x.HostId == hostId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.ProjectId,
                    x.Project.ProjectName,
                    x.BatchId,
                    x.Batch.BatchName,
                    x.HostId,
                    x.ParticipantId,
                    x.Participant.FirstName,
                    x.Participant.LastName,
                    IsAnyExistingConferenceBetweenThem = _context.Conference.Any(c=> c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going"),
                    RoomId = _context.Conference.Where(c => c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.RoomId).FirstOrDefault()

                }).ToListAsync();


                var count = await query.CountAsync();

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }

        }

        public async Task<object> GetOnGoingConferenceByHostId(string hostId)
        {


            try
            {
                var query = _context.Conference.Where(x => x.HostId == hostId && x.Status == "On-Going").AsQueryable();
                var data = await query.OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.RoomId,
                    x.HostId,
                    x.ParticipantId,
                    x.Status,
                    Host = x.Host.FirstName,
                    Participant = x.Participant.FirstName

                }).FirstAsync();

                return new
                {
                    Success = true,
                    CurrentConfRoomId = data.RoomId

                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }

        }

        // Participant Side
        public async Task<object> GetHostListByParticipantId(string participantId)
        {


            try
            {
                var query = _context.BatchHostParticipant.Where(x => x.ParticipantId == participantId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.ProjectId,
                    x.Project.ProjectName,
                    x.BatchId,
                    x.Batch.BatchName,
                    x.HostId,
                    x.ParticipantId,
                    x.Host.FirstName,
                    x.Host.LastName,
                    IsAnyExistingConferenceBetweenThem = _context.Conference.Any(c => c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going"),
                    RoomId = _context.Conference.Where(c => c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.RoomId).FirstOrDefault()

                }).ToListAsync();


                var count = await query.CountAsync();

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }

        }



        public async Task<object> CreateConference(Conference confObj)
        {
            
            try
            {
                bool isSameRoomAleadyCreated = IsSameRoomAleadyCreated(confObj.RoomId);

                if (isSameRoomAleadyCreated)
                {
                    return new
                    {
                        Success = false,
                        Message = "A conference with same name has been identified. Action Aborted ! Please contact system administrators."
                    };
                }

                bool isThereAnyOnGoingMeeting = IsThereAnyOnGoingMeeting(confObj.HostId);

                if (isThereAnyOnGoingMeeting)
                {
                    return new
                    {
                        Success = false,
                        Message = "You have currently one existing conference. Hence, you can not start another one. You are requested to join in your previously created cnference."
                    };
                }


                
                Helpers h = new Common.Helpers(_context);

                var newlastRoomNumber = h.GenerateRoomNumber();


                // string lastRoomNumber = _context.Conference.OrderByDescending(x => x.Id).Select(x => x.RoomId).FirstOrDefault();
                // if (lastRoomNumber == null)
                // {
                //     return "Room-101";
                // }
                // var splitItems = lastRoomNumber.Split(new string[] { "Room-" }, StringSplitOptions.None);
                // int lastRoomNumberInt = Convert.ToInt32(splitItems[1]);
                // int newlastRoomNumberInt = lastRoomNumberInt + 1;
                // lastRoomNumber = "Room-" + Convert.ToString(newlastRoomNumberInt);

                confObj.RoomId = newlastRoomNumber;
                confObj.CreatedDateTime = DateTime.UtcNow;
                confObj.Status = "On-Going";
                _context.Conference.Add(confObj);
                await _context.SaveChangesAsync();


                string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();
                // Now, signalR comes into play
                await _notificationHubContext.Clients.All.SendAsync("Created", "2b298d72-f6f3-4a39-8ea1-683e5880f7f9");

                return new
                {
                    Success = true,
                    Message = "Successfully conference created !",
                    CurrentConfRoomId = confObj.RoomId,
                    ConferenceId = confObj.Id,
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    ex.Message
                };
            }
        }

        public async Task<object> EndConference(Conference confObj)
        {

            try
            {


                Conference existingConf =
                    _context.Conference.Where(x => x.RoomId == confObj.RoomId).Select(x => x).FirstOrDefault();

                if (existingConf != null)
                {
                    existingConf.Status = "Closed";
                    await _context.SaveChangesAsync();


                    string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();

                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("Ended", "2b298d72-f6f3-4a39-8ea1-683e5880f7f9");


                    return new
                    {
                        Success = true,
                        Message = "Successfully conference ended !"
                    };
                }

                return new
                {
                    Success = false,
                    Message = "No conference found !"
                };

                


            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    ex.Message
                };
            }
        }


        public async Task<object> JoinConferenceByHost(Conference confObj)
        {

            try
            {                
                Conference existingConf =
                    _context.Conference.Where(x => x.RoomId == confObj.RoomId).Select(x => x).FirstOrDefault();

                if (existingConf != null)
                {

                    string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();
                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("Joined", myParticipantId);


                    return new
                    {
                        Success = true,
                        Message = "Successfully conference joined by Host !"
                    };
                }

                return new
                {
                    Success = false,
                    Message = "No conference found !"
                };
                


            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    ex.Message
                };
            }
        }
        public async Task<object> GetConferenceList()
        {
            try
            {
                var query = _context.Conference.Where(x => x.Status != "Finished").AsQueryable();
                var data = await query.OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.RoomId,
                    x.HostId,
                    x.ParticipantId,
                    x.Status,

                }).ToListAsync();

                var count = await query.CountAsync();

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
                    Message = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : ex.Message
                };
            }
        }
        
    }
}