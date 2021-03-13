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
                    HasJoinedByHost = _context.Conference.Where(c=> c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.HasJoinedByHost).FirstOrDefault(),
                    HasJoinedByParticipant = _context.Conference.Where(c=> c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.HasJoinedByParticipant).FirstOrDefault(),
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
                    x.HasJoinedByHost,
                    x.HasJoinedByParticipant,
                    Host = x.Host.FirstName,
                    Participant = x.Participant.FirstName,
                    x.CreatedDateTime,
                    x.BatchId

                }).FirstAsync();

                return new
                {
                    Success = true,
                    CurrentConference = data

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
                    HasJoinedByHost = _context.Conference.Where(c=> c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.HasJoinedByHost).FirstOrDefault(),
                    HasJoinedByParticipant = _context.Conference.Where(c=> c.HostId == x.HostId && c.ParticipantId == x.ParticipantId && c.Status == "On-Going").OrderByDescending(c => c.Id).Select(c => c.HasJoinedByParticipant).FirstOrDefault(),
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

                string newlastRoomNumber = h.GenerateRoomNumber();


                //string newlastRoomNumber = CreateUniqueRoomId();


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
                confObj.HasJoinedByHost= false;
                confObj.HasJoinedByParticipant = false;
                confObj.Status = "On-Going";
                _context.Conference.Add(confObj);
                await _context.SaveChangesAsync();


                string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();
                // Now, signalR comes into play
                await _notificationHubContext.Clients.All.SendAsync("Created", myParticipantId);

                return new
                {
                    Success = true,
                    Message = "Successfully conference created !",
                    CurrentConfRoomId = confObj.RoomId,
                    CurrentConference = confObj
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

        private string CreateUniqueRoomId()
        {

            //var roomId = _context.Conferences
            //    .OrderBy(x=>x.Id)
            //    .Select(x => x.RoomId)
            //    .LastOrDefault()

            var roomId = _context.Conference
                .OrderByDescending(x => x.Id)
                .Select(x => x.RoomId)
                .FirstOrDefault();
            if (roomId == null)
            {
                return "Room-101";
            }
            var roomNum = int.Parse(roomId.Remove(0, 5));
            roomNum++;
            return "Room-" + roomNum;
           
        
        }


        public async Task<object> JoinConferenceByHost(Conference confObj)
        {

            try
            {                
                Conference existingConf =
                    _context.Conference
                    .Where(x => 
                    x.HostId == confObj.HostId && 
                    x.ParticipantId == confObj.ParticipantId && 
                    x.RoomId == confObj.RoomId && 
                    x.Status == "On-Going")
                    .Select(x => x

                    ).FirstOrDefault();

                if (existingConf != null)
                {

                    // string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();
                    if(existingConf.HasJoinedByHost){
                        return new{
                            Success = false,
                            Message = "You have already joined the meeting in another browser, multiple joining NOT allowed !"

                        };
                    }

                    existingConf.HasJoinedByHost = true;
                    await _context.SaveChangesAsync();

                    

                   
                    ConferenceHistory confHistoryObj = new ConferenceHistory();
                    confHistoryObj.ConferenceId = existingConf.Id;
                    confHistoryObj.RoomId = existingConf.RoomId;
                    confHistoryObj.HostId = existingConf.HostId;
                    confHistoryObj.JoineDateTime = DateTime.UtcNow;
                    confHistoryObj.ConnectionId = confObj.ConnectionId;

                    _context.ConferenceHistory.Add(confHistoryObj);
                    await _context.SaveChangesAsync();



                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("Joined", existingConf.ParticipantId);


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

        public async Task<object> EndConference(Conference confObj)
        {

            try
            {


                Conference existingConf =
                    _context.Conference.Where(x => 
                    x.HostId == confObj.HostId && 
                    x.ParticipantId == confObj.ParticipantId && 
                    x.RoomId == confObj.RoomId && 
                    x.Status == "On-Going").Select(x => x).FirstOrDefault();

                if (existingConf != null)
                {

                    string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();


                    existingConf.Status = "Closed";
                    existingConf.HasJoinedByHost = false;
                    existingConf.HasJoinedByParticipant = false;
                    await _context.SaveChangesAsync();




                    //Host LeaveDatetime setting
                    ConferenceHistory confHistoryObj = _context.ConferenceHistory.Where(c=> c.ConferenceId == existingConf.Id && c.HostId == existingConf.HostId).Select(c=> c).FirstOrDefault();
                    if(confHistoryObj !=null){
                        confHistoryObj.LeaveDateTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    //Participant LeaveDatetime setting
                    ConferenceHistory confHistoryObj2 = _context.ConferenceHistory.Where(c=> c.ConferenceId == existingConf.Id && c.ParticipantId == existingConf.ParticipantId).Select(c=> c).OrderByDescending(c => c.Id).FirstOrDefault();
                    if(confHistoryObj2 !=null && confHistoryObj2.LeaveDateTime ==null){
                        confHistoryObj2.LeaveDateTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    


                   

                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("Ended", myParticipantId);
                    await _notificationHubContext.Clients.All.SendAsync("LetHostKnowConferenceEnded", confObj.HostId); //this is needed if multiple browsers opened


                    return new
                    {
                        Success = true,
                        Message = "Successfully conference ended !",
                        ParticipantId =myParticipantId
                    };
                }

                return new
                {
                    Success = false,
                    Message = "No On-Going conference found !"
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

        public async Task<object> EndConferenceByParticipant(Conference confObj)
        {

            Console.WriteLine(confObj.RoomId);

            try
            {


                    ConferenceHistory confHistoryObj = _context.ConferenceHistory
                    .Where(c=> c.RoomId == confObj.RoomId && c.ParticipantId == confObj.ParticipantId && c.LeaveDateTime ==null)
                    .Select(c=> c).OrderByDescending(c => c.Id)
                    .FirstOrDefault();

                    if(confHistoryObj !=null){
                        confHistoryObj.LeaveDateTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                    }
                    else{
                        return new
                        {
                            Success = false,
                            Message = "Participant 'LeaveDateTime' is already set."
                        };
                    }


                    Conference existingConf =
                    _context.Conference.Where(x => 
                    x.HostId == confObj.HostId && 
                    x.ParticipantId == confObj.ParticipantId && 
                    x.RoomId == confObj.RoomId && 
                    x.Status == "On-Going").Select(x => x).FirstOrDefault();

                    if (existingConf != null)
                    {

                        existingConf.HasJoinedByParticipant = false;
                        await _context.SaveChangesAsync();

                    }


                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("EndedByParticipant", confObj.HostId);
                    await _notificationHubContext.Clients.All.SendAsync("LetParticipantKnowConferenceEnded", confObj.ParticipantId); // this is needed if multiple browsers opened


                    return new
                    {
                        Success = true,
                        Message = "Successfully conference ended !"
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

            // try
            // {


            //     Conference existingConf =
            //         _context.Conference.Where(x => 
            //         x.HostId == confObj.HostId && 
            //         x.ParticipantId == confObj.ParticipantId && 
            //         x.RoomId == confObj.RoomId && 
            //         x.Status == "On-Going").Select(x => x).FirstOrDefault();

            //     if (existingConf != null)
            //     {

            //         // string myParticipantId = _context.Conference.Where(c=>c.HostId == confObj.HostId && c.Status == "On-Going").Select(c=> c.ParticipantId).FirstOrDefault();


            //         // existingConf.Status = "Closed";
            //         // await _context.SaveChangesAsync();





            //         ConferenceHistory confHistoryObj = _context.ConferenceHistory
            //         .Where(c=> c.ConferenceId == existingConf.Id && c.ParticipantId == existingConf.ParticipantId && c.LeaveDateTime ==null)
            //         .Select(c=> c).OrderByDescending(c => c.Id)
            //         .FirstOrDefault();

            //         if(confHistoryObj !=null){
            //             confHistoryObj.LeaveDateTime = DateTime.UtcNow;
            //             await _context.SaveChangesAsync();

            //         }
            //         else{
            //             return new
            //             {
            //                 Success = false,
            //                 Message = "Participant 'LeaveDateTime' is already set."
            //             };
            //         }
                    
                    


                   

            //         // Now, signalR comes into play
            //         await _notificationHubContext.Clients.All.SendAsync("EndedByParticipant", existingConf.HostId);


            //         return new
            //         {
            //             Success = true,
            //             Message = "Successfully conference ended !"
            //         };
            //     }

            //     return new
            //     {
            //         Success = false,
            //         Message = "No On-Going conference found !"
            //     };

                


            // }
            // catch (Exception ex)
            // {
            //     return new
            //     {
            //         Success = false,
            //         ex.Message
            //     };
            // }
        }

        public async Task<object> JoinConferenceByParticipant(Conference confObj)
        {

            try
            {                
                var existingConf =
                    _context.Conference
                    .Where(x => 
                    x.HostId == confObj.HostId && 
                    x.ParticipantId == confObj.ParticipantId && 
                    x.RoomId == confObj.RoomId && 
                    x.Status == "On-Going")
                    .Select(x => x).FirstOrDefault();

                if (existingConf != null)
                {
                    if(existingConf.HasJoinedByParticipant){
                        return new{
                            Success = false,
                            Message = "You have already joined the meeting in another browser, multiple joining NOT allowed !"

                        };
                    }

                    existingConf.HasJoinedByParticipant = true;
                    await _context.SaveChangesAsync();


                    ConferenceHistory confHistoryObj = new ConferenceHistory();
                    confHistoryObj.ConferenceId = existingConf.Id;
                    confHistoryObj.RoomId = existingConf.RoomId;
                    confHistoryObj.ParticipantId = existingConf.ParticipantId;
                    confHistoryObj.JoineDateTime = DateTime.UtcNow;
                    confHistoryObj.ConnectionId = confObj.ConnectionId;

                    _context.ConferenceHistory.Add(confHistoryObj);
                    await _context.SaveChangesAsync();


                    return new
                    {
                        Success = true,
                        Message = "Conference joined successfully !"
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
        
        public async Task<object> GetCallingHistoryByDaterange(DateTimeParams obj)
        {
            Helpers h = new Helpers(_context);


            // var univStartDate = obj.StartDate.ToUniversalTime();
            // var univEndDate = obj.EndDate.ToUniversalTime();

            var startDate = obj.StartDate;
            var endDate = obj.EndDate.AddDays(1).AddTicks(-1);

            Console.WriteLine(startDate);
            Console.WriteLine(endDate);

            

            var data =  await _context.ConferenceHistory
            .Where(cs => cs.JoineDateTime >= startDate && cs.LeaveDateTime <= endDate && cs.HostId !=null)
            .Select(cs => new{
                    Id = cs.ConferenceId,
                    cs.RoomId,
                    cs.HostId,
                    HostFirstName = cs.Host.FirstName,
                    // cs.ParticipantId,
                    // ParticipantFirstName = cs.Participant.FirstName,
                    ConferenceCallDetail = h.GetEffectiveCallDurationBetweenHostAndParticipant(cs.ConferenceId),
            }).ToListAsync(); 
            
            

            return new
            {
                Success = true,
                Records = data
            };
            
        }

        public async Task<object> GetConferenceHistoryDetailById(long confId)
        {

            Helpers h = new Helpers(_context);

            var data =  await _context.ConferenceHistory
            .Where(cs => cs.ConferenceId == confId)
            .Select(cs => new{
                    Id = cs.ConferenceId,
                    cs.RoomId,
                    cs.HostId,
                    HostFirstName = cs.Host.FirstName,
                    cs.ParticipantId,
                    ParticipantFirstName = cs.Participant.FirstName,
                    cs.JoineDateTime,
                    cs.LeaveDateTime
            }).ToListAsync(); 

            return new
            {
                Success = true,
                Records = data
            };
            
        }
        

        public async Task<object> TestApi()
        {

            await _context.Command.Select(c => c).ToListAsync();

            //=======================================================

            //     TimeRange timeRange1 = new TimeRange(
            //         new DateTime( 2011, 2, 22, 12, 0, 0 ),
            //         new DateTime( 2011, 2, 22, 16, 0, 0 ) );
            //     Console.WriteLine( "TimeRange1: " + timeRange1 );


            //     TimeRange timeRange2 = new TimeRange(
            //         new DateTime( 2011, 2, 22, 15, 0, 0 ),
            //         new DateTime( 2011, 2, 22, 18, 0, 0 ) );
            //     Console.WriteLine( "TimeRange2: " + timeRange2 );


            //     TimeRange timeRange3 = new TimeRange(
            //         new DateTime( 2011, 2, 22, 15, 0, 0 ),
            //         new DateTime( 2011, 2, 22, 21, 0, 0 ) );
            //     Console.WriteLine( "TimeRange3: " + timeRange3 );

            //     Console.WriteLine( "Relation Between TimeRange 1 and TimeRange 2 : " +
            //          timeRange1.GetRelation( timeRange2 ) );


            //     Console.WriteLine( "Relation Between TimeRange 1 and TimeRange 3 : " +
            //                         timeRange1.GetRelation( timeRange3 ) );
                
            //     Console.WriteLine( "Relation Between TimeRange 3 and TimeRange 2 : "+
            //                         timeRange3.GetRelation( timeRange2 ) );


            //     // --- intersection ---
            //     Console.WriteLine( "TimeRange1.GetIntersection( TimeRange2 ): " +
            //                         timeRange1.GetIntersection( timeRange2 ) );

            //     // --- intersection ---
            //     Console.WriteLine( "TimeRange2.GetIntersection( TimeRange3 ): " +
            //                         timeRange2.GetIntersection( timeRange3 ) );

                
            //     var a = timeRange2.GetIntersection( timeRange3 );
            //     Console.WriteLine(a);


            // //    var hostDate = new DateTime(2008, 3, 1, 7, 0, 0);
            // //    var participantDate =  new DateTime(2008, 3, 1, 7, 0, 0);

            // //    Tuple<DateTime, DateTime> ranges = new Tuple<DateTime, DateTime>(hostDate,participantDate);
               
               
            // //    bool result = Overlap(ranges);
            // //    Console.WriteLine(result);

            // var hostStart = new DateTime(2008, 3, 1, 7, 0, 0);
            // var hostEnd = new DateTime(2011, 3, 1, 7, 0, 0);

            // var participantStart = new DateTime(2012, 3, 1, 13, 15, 20);
            // var participantEnd = new DateTime(2012, 3, 1, 13, 27, 25);

            // // DateTime a = new DateTime(2010, 05, 12, 13, 15, 00);
            // // DateTime b = new DateTime(2010, 05, 12, 13, 45, 00);


            // bool result = intersects(hostStart, hostEnd, participantStart, participantEnd);
            // Console.WriteLine(result);

            // TimeSpan diff = participantEnd - participantStart;
            // Console.WriteLine(diff);



            //=======================================================
            // TimeSpan result = new TimeSpan();  

            var confHistoryObj = _context.ConferenceHistory
            .Where(cs => cs.ConferenceId == 305)
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
                    Console.WriteLine("-----------------Participant start :" + particpantStartDate + " " + "to Participant end :" + particpantEndDate + " range is inside calling Host call duration");
                    diff = particpantEndDate - particpantStartDate;
                    Console.WriteLine("-------------------------Participant start :" + particpantStartDate);
                    Console.WriteLine("-------------------------Participant end :" + particpantEndDate);
                    Console.WriteLine("-----------------Diferrence :" + diff);
                    actualCallDuration = actualCallDuration + diff;
                }

            }

            Console.WriteLine("How many times participant joined the conference? : " + count);
            Console.WriteLine("Actual Call Duration : " + actualCallDuration);

            return new
            {
                Success = true,
            };
            
        }
        
    }
}


