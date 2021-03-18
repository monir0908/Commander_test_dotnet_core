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

        private bool IsSameVirtualClassAleadyCreated(string roomId)
        {
            return _context.VClass.Any(x => x.RoomId == roomId);
        }

        private bool IsThereAnyOnGoingVirtualClass(string hostId)
        {
            return _context.VClass.Any(c => c.HostId == hostId && c.Status == "On-Going");
        }

        private bool IsParticipantCurrentlyInVirtualClass(string participantId)
        {
            return _context.VClassDetail.Any(c => c.ParticipantId == participantId && c.LeaveTime == null);
        }

        private bool IsVirtualClassJoinable(long vclassId){            

            return _context.VClass.Any(c => c.Id == vclassId && c.Status == "On-Going");

        }
        private bool HasHostJoinedTheVirtualClass(VClassDetail obj)
        {
            Console.WriteLine("Host ID: ");
            Console.WriteLine(obj.HostId);

            Console.WriteLine("Class ID: ");
            Console.WriteLine(obj.VClassId);

            Console.WriteLine("Room ID: ");
            Console.WriteLine(obj.RoomId);

            return _context.VClassDetail.Any(c => c.HostId == obj.HostId && c.VClassId == obj.VClassId && c.RoomId == obj.RoomId && c.LeaveTime == null);
        }

        private bool HasParticipantJoinedTheVirtualClass(VClassDetail obj)
        {
            Console.WriteLine("Host ID: ");
            Console.WriteLine(obj.HostId);

            Console.WriteLine("Class ID: ");
            Console.WriteLine(obj.VClassId);

            Console.WriteLine("Room ID: ");
            Console.WriteLine(obj.RoomId);

            return _context.VClassDetail.Any(c => c.ParticipantId == obj.ParticipantId && c.VClassId == obj.VClassId && c.RoomId == obj.RoomId && c.LeaveTime == null);
        }
        private void SendInvitationForVirtualClass(VClassDetail vClassDetail, IEnumerable<ParticipantList> participantList)
        {
            
            foreach (var item in participantList)
            {
                VClassInvitation viObj = new VClassInvitation();
                viObj.VClassId = vClassDetail.VClassId;
                viObj.RoomId = vClassDetail.RoomId ;
                viObj.BatchId = vClassDetail.BatchId ;
                viObj.HostId = vClassDetail.HostId ;
                viObj.InvitationDateTime = DateTime.UtcNow;
                viObj.Status = "Invited";
                viObj.ParticipantId = item.Id;

                _context.VClassInvitation.Add(viObj);
                _context.SaveChanges();
            }
            return;
            
        }

        private void RemoveInvitationList(long vClassId)
        {
            
            _context.VClassInvitation.RemoveRange(_context.VClassInvitation.Where(x => x.VClassId == vClassId && x.Status == "Invited"));
            _context.SaveChanges();   
            return;
            
        }
        


        // Host Side

        public async Task<object> GetProjectListByHostId(string hostId)
        {
            try
            {

                
               var query =  _context.ProjectBatchHost.Where(x => x.HostId == hostId).AsQueryable();

               var data = await query.Join(_context.ProjectBatch,
               x => x.ProjectBatchId,
               y => y.Id,
               (x, y) => new { ProjectBatchHost = x, ProjectBatch = y })
               .Join(_context.Project,
               x => x.ProjectBatch.ProjectId,
               y => y.Id,
               (x, y) => new { x.ProjectBatchHost, x.ProjectBatch, Project = y })
               .Select(x => new
               {
                   Id = x.Project.Id,
                   ProjectName = x.Project.ProjectName,
               }).Distinct().ToListAsync();

              


               var count = data.Count;

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
                var query = _context.ProjectBatch.Where(x => x.ProjectId == pId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
                    x.Batch.BatchName,
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


        public async Task<object> GetBatchListByProjectIdAndHostId(long pId, string hostId)
        {


            try
            {
                long[] bIds = _context.ProjectBatch.Where(x => x.ProjectId == pId).Select(x => x.Id).ToArray();
                
                
                var data = await _context.ProjectBatchHost.Where(x => bIds.Contains(x.ProjectBatchId) && x.HostId == hostId)
                .Join(_context.ProjectBatch,
                pbh => pbh.ProjectBatchId,
                pb => pb.Id,
                (x, y) => new { ProjectBatchHost = x, ProjectBatch = y })                
                .Select(x => new{
                    Id = x.ProjectBatch.Batch.Id,
                    BatchName = x.ProjectBatch.Batch.BatchName
                }).ToListAsync();


                var count = data.Count;

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
                var query = _context.ProjectBatchHostParticipant.Where(x => x.ParticipantId == "fdsfdsfd").AsQueryable();
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
                var query = _context.ProjectBatchHostParticipant.Where(x => x.ParticipantId == "dfsdfdsf").AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
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
                var query = _context.ProjectBatchHostParticipant.Where(x => x.ParticipantId == participantId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id).Select(x => new
                {
                    x.Id,
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
        
        //==============================================================================


        public async Task<object> GetParticipantListByProjectIdBatchIdAndHostId(long projectId, long batchId, string hostId)
        {


            try
            {
                long [] pbIds = _context.ProjectBatch.Where(x => x.ProjectId == projectId && x.BatchId == batchId).Select(x => x.Id).ToArray();
                
                foreach (var item in pbIds)
                {
                    Console.WriteLine(item);
                }

                

                long[] pbhIds = _context.ProjectBatchHost.Where(x => pbIds.Contains(x.ProjectBatchId) && x.HostId == hostId).Select(x => x.Id).ToArray();


                foreach (var item in pbhIds)
                {
                    Console.WriteLine(item);
                }
                var data = await _context.ProjectBatchHostParticipant.Where(x => pbhIds.Contains(x.ProjectBatchHostId))
                .Select(x => new
                {
                    x.ParticipantId,
                    x.Participant.FirstName,
                    x.Participant.LastName,
                    
                }).ToListAsync();


                var count = data.Count;

                return new
                {
                    Success = true,
                    Records = data,
                    Total = count,
                    pbIds = pbIds
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

        
        public async Task<object> GetCurrentOnGoingVirtualClassListByHostId(string hostId){
            try
            {
                var query = _context.VClass.Where(x => x.Status == "On-Going" && x.HostId == hostId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.RoomId,
                    x.HostId,
                    x.BatchId,
                    x.Batch.BatchName,                    
                    x.Status,
                    HasJoinedByHost = _context.VClassDetail.Any(c => c.HostId == x.HostId && c.VClassId == x.Id && c.RoomId == x.RoomId && c.LeaveTime == null),

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

        public async Task<object> GetInvitationListByParticipantId(string participantId){
            try
            {
                var query = _context.VClassInvitation.Where(x => x.Status == "Invited" && x.ParticipantId == participantId).AsQueryable();
                var data = await query.OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.VClassId,
                    x.RoomId,
                    x.ParticipantId,
                    x.HostId,
                    HostFirstName= x.Host.FirstName,
                    HostLastName= x.Host.LastName,
                    x.BatchId,
                    x.Batch.BatchName,                    
                    x.Status,
                    HasJoinedByParticipant = _context.VClassDetail.Any(c => c.ParticipantId == participantId && c.VClassId == x.VClassId && c.RoomId == x.RoomId && c.LeaveTime == null),

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



        public async Task<object> CreateVirtualClass(VClass vClassObj)
        {
            
            try
            {
                bool isSameVirtualClassAleadyCreated = IsSameVirtualClassAleadyCreated(vClassObj.RoomId);

                if (isSameVirtualClassAleadyCreated)
                {
                    return new
                    {
                        Success = false,
                        Message = "A class with same name has been identified. Action Aborted ! Please contact system administrators."
                    };
                }

                bool isThereAnyOnGoingVirtualClass = IsThereAnyOnGoingVirtualClass(vClassObj.HostId);

                if (isThereAnyOnGoingVirtualClass)
                {
                    return new
                    {
                        Success = false,
                        Message = "You have currently one existing conference. Hence, you can not start another one. You are requested to join in your previously created cnference."
                    };
                }


                
                Helpers h = new Common.Helpers(_context);

                string newlastRoomNumber = h.GenerateVirtualClassRoomNumber();

                vClassObj.RoomId = newlastRoomNumber;
                vClassObj.CreatedDateTime = DateTime.UtcNow;
                vClassObj.Status = "On-Going";
                _context.VClass.Add(vClassObj);
                await _context.SaveChangesAsync();

                return new
                {
                    Success = true,
                    Records = vClassObj,
                    Message = "Successfully " + Helpers.GlobalProperty +" created !"
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
        
        public async Task<object> JoinVirtualClassByHost(VClassDetail vClassDetail, IEnumerable<ParticipantList> participantList)
        {
            bool isVirtualClassJoinable = IsVirtualClassJoinable(vClassDetail.VClassId);
            if(isVirtualClassJoinable == false){
                    return new
                    {
                        Success = false,
                        Message = "It seems, the virtual class has been closed by you !"

                    };
                }



            try
            {   
                bool hasHostJoinedTheVirtualClass = HasHostJoinedTheVirtualClass(vClassDetail);
                // Console.WriteLine(hasHostJoinedTheVirtualClass);

                if(hasHostJoinedTheVirtualClass){
                    return new
                    {
                        Success = false,
                        Message = "It seems you joined the class in another browser, multiple joining NOT allowed !"

                    };
                }

                else
                {

                    vClassDetail.JoinTime = DateTime.UtcNow;
                    _context.VClassDetail.Add(vClassDetail);
                    await _context.SaveChangesAsync();

                    
                    var HostDetail = _context.VClassDetail.Where(x => x.VClassId == vClassDetail.VClassId && x.HostId == vClassDetail.HostId)
                    .Select(x => new{
                        HostFullName = x.Host.FirstName + " " + x.Host.LastName,
                        x.RoomId,
                    }).FirstOrDefault();

                    Console.WriteLine(HostDetail.HostFullName);

                    
                    foreach (var participant in participantList)
                    {
                        // Now, signalR comes into play
                        await _notificationHubContext.Clients.All.SendAsync("JoinedByHost", participant.Id, HostDetail.RoomId, HostDetail.HostFullName);
                        
                    }

                    SendInvitationForVirtualClass(vClassDetail, participantList);
                    return new
                    {
                        Success = true,
                        Records = vClassDetail,
                        Message = "You successfully joined class !"
                    };

                }   


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

        public async Task<object> JoinVirtualClassByParticipant(VClassDetail vClassDetail)
        {
            bool isVirtualClassJoinable = IsVirtualClassJoinable(vClassDetail.VClassId);
            if(isVirtualClassJoinable == false){
                    return new
                    {
                        Success = false,
                        Message = "It seems, the virtual class has been closed by Teacher !"

                    };
                }

            try
            {                
                bool hasParticipantJoinedTheVirtualClass = HasParticipantJoinedTheVirtualClass(vClassDetail);
                Console.WriteLine(hasParticipantJoinedTheVirtualClass);

                if(hasParticipantJoinedTheVirtualClass){
                    return new
                    {
                        Success = false,
                        Message = "It seems you joined the class in another browser, multiple joining NOT allowed !"

                    };
                }
                else if(IsParticipantCurrentlyInVirtualClass(vClassDetail.ParticipantId))
                {
                    return new
                    {
                        Success = false,
                        Message = "It seems, you currently in a call. Therefore, you won't be able to join in another class !"

                    };
                    
                }

                else
                {

                    vClassDetail.JoinTime = DateTime.UtcNow;
                    _context.VClassDetail.Add(vClassDetail);
                    await _context.SaveChangesAsync();

                    return new
                    {
                        Success = true,
                        Records = vClassDetail,
                        Message = "Class joined successfully !"
                    };

                }   
                


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

        public async Task<object> EndVirtualClassByHost(VClass vClassObj)
        {

            try
            {
                VClass existingConf =
                    _context.VClass.Where(x =>
                    x.Id == vClassObj.Id && 
                    x.HostId == vClassObj.HostId  && 
                    x.RoomId == vClassObj.RoomId && 
                    x.Status == "On-Going")
                    .Select(x => x)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();                    

                if (existingConf != null)
                {

                    //Host LeaveTime updating
                    VClassDetail vClassDetail = _context.VClassDetail.Where(c=> c.VClassId == existingConf.Id && c.HostId == existingConf.HostId).Select(c=> c).OrderByDescending(c => c.Id).FirstOrDefault();
                    if(vClassDetail !=null){
                        vClassDetail.LeaveTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    //Getting Participants and LeaveTime updating
                    var participantList = _context.VClassDetail.Where(c=> c.VClassId == existingConf.Id && c.ParticipantId !=null && c.LeaveTime ==null).Select(c=> c).ToList();

                    
                    if(participantList.Count() >0){
                        foreach (var obj in participantList)
                        {
                            obj.LeaveTime = DateTime.UtcNow;
                            await _context.SaveChangesAsync();

                            // Now, signalR comes into play
                            // await _notificationHubContext.Clients.All.SendAsync("EndedByHost", obj.ParticipantId);
                            
                        }                       
                        
                    }

                   


                    // Letting all invitess know if class has been ended by host
                    var invitees = _context.VClassInvitation.Where(c=> c.VClassId == existingConf.Id && c.Status == "Invited").Select(c=> new{
                        c.ParticipantId,
                        HostFullName = c.Host.FirstName + " " + c.Host.LastName,
                        c.RoomId,
                    }).ToList();


                    foreach (var item in invitees)
                    {
                        
                        await _notificationHubContext.Clients.All.SendAsync("EndedByHost", item.ParticipantId, item.RoomId, item.HostFullName);
                        
                    }



                    // Removing all invitation

                    RemoveInvitationList(vClassDetail.VClassId);

                    // Now, signalR comes into play
                    await _notificationHubContext.Clients.All.SendAsync("LetHostKnowClassEnded", vClassObj.HostId); //this is needed if multiple browsers opened

                    // Now, update 'VClass' table with call-details
                    Helpers h = new Helpers(_context);
                    var res = h.GetActualCallDurationBetweenHostAndParticipant(vClassObj.Id);
                    existingConf.Status = "Closed";

                    existingConf.HostCallDuration = res.HostCallDuration;
                    existingConf.ParticipantsCallDuration = res.ParticipantsCallDuration;
                    existingConf.EmptySlotDuration = res.EmptySlotDuration;
                    existingConf.ActualCallDuration = res.ActualCallDuration;
                    existingConf.ParticipantJoined = res.ParticipantJoined;
                    existingConf.UniqueParticipantCounts = res.UniqueParticipantCounts;
                    await _context.SaveChangesAsync();

                    return new
                    {
                        Success = true,
                        Message = "Successfully class ended !"
                    };
                }

                return new
                {
                    Success = false,
                    Message = "No on-going class found !"
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

        public async Task<object> EndVirtualClassByParticipant(VClassDetail vClassDetail)
        {
            try
            {


                    VClassDetail detailObj = 
                    _context.VClassDetail
                    .Where(c=> c.VClassId == vClassDetail.Id && c.ParticipantId == vClassDetail.ParticipantId)
                    .Select(c=> c)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();

                    

                    if(detailObj !=null && detailObj.LeaveTime == null){
                        detailObj.LeaveTime = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                    }
                    else{
                        return new
                        {
                            Success = false,
                            Message = "The virtual class has already been ended by your Teacher."
                        };
                    }                    


                    // Now, signalR comes into play
                    // This is needed if multiple browsers opened by this participant
                    await _notificationHubContext.Clients.All.SendAsync("LetParticipantKnowClassEnded", vClassDetail.ParticipantId); 


                    return new
                    {
                        Success = true,
                        Message = "Successfully class ended !"
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

        public async Task<object> GetVirtualClassCallingHistoryByDaterange(DateTimeParams obj)
        {
            Helpers h = new Helpers(_context);
            

            var startDate = obj.StartDate;
            var endDate = obj.EndDate.AddDays(1).AddTicks(-1);

            Console.WriteLine(startDate);
            Console.WriteLine(endDate);

            

            var data =  await _context.VClassDetail
            .Where(cs => cs.JoinTime >= startDate && cs.LeaveTime <= endDate && cs.HostId !=null)
            .Select(cs => new{
                    Id = cs.VClassId,
                    cs.RoomId,
                    cs.HostId,
                    HostFirstName = cs.Host.FirstName,
                    // cs.ParticipantId,
                    // ParticipantFirstName = cs.Participant.FirstName,
                    ConferenceCallDetail = h.GetActualCallDurationBetweenHostAndParticipant(cs.VClassId),
            }).ToListAsync(); 
            
            

            return new
            {
                Success = true,
                Records = data
            };
            
        }

        public async Task<object> GetVirtualClassDetailById(long vclassId)
        {

            Helpers h = new Helpers(_context);

            var data =  await _context.VClass
            .Where(cs => cs.Id == vclassId)
            .Select(cs => new{
                    cs.Id,
                    cs.RoomId,
                    cs.HostId,
                    HostFirstName = cs.Host.FirstName,
                    cs.HostCallDuration,
                    cs.ParticipantsCallDuration,
                    cs.EmptySlotDuration,
                    cs.ActualCallDuration,
                    cs.ParticipantJoined,
                    cs.UniqueParticipantCounts,
            }).ToListAsync(); 

            return new
            {
                Success = true,
                Records = data
            };
            
        }
        
        public async Task<object> TestApi()
        {


            await _context.Project.Select(c => c).ToListAsync();

            var participantList =   _context.VClassDetail.Where(c=> c.VClassId == 1 && c.ParticipantId !=null).Select(c=> c).ToList();

            var ids = participantList.Select(x => x.Id).ToArray();           
            


            return new  
            {
                Success = true,
                Data = ids,
            };
            
        }
        
    }
}


