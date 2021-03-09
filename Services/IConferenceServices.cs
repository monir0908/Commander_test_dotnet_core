using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commander.Models;
using Microsoft.EntityFrameworkCore;
using Commander.Common;
using Microsoft.Extensions.Configuration;

namespace Commander.Services{


    public interface IConferenceServices
    {
        Task<object> GetProjectListByHostId(string hostId);
        Task<object> GetBatchListByProjectId(long pId);
        Task<object> GetParticipantListByBatchId(long batchId);
        Task<object> GetParticipantListByHostId(string hostId);
        Task<object> GetOnGoingConferenceByHostId(string hostId);


        Task<object> GetHostListByParticipantId(string participantId);


        Task<object> CreateConference(Conference confObj);
        Task<object> JoinConferenceByHost(Conference confObj);
        Task<object> JoinConferenceByParticipant(Conference confObj);
        Task<object> EndConference(Conference confObj);
        Task<object> EndConferenceByParticipant(Conference confObj);
        Task<object> GetConferenceList();
        Task<object> TestApi();
        Task<object> GetCallingHistoryByDaterange(DateTimeParams obj);
        Task<object> GetConferenceHistoryDetailById(long confId);

    }
}