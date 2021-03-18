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
        Task<object> GetBatchListByProjectIdAndHostId(long pId, string hostId);



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
        Task<object> GetCallingHistoryByDaterange(DateTimeParams obj);
        Task<object> GetConferenceHistoryDetailById(long confId);




        Task<object> GetParticipantListByProjectIdBatchIdAndHostId(long projectId, long batchId, string hostId);
        Task<object> GetCurrentOnGoingVirtualClassListByHostId(string hostId);
        Task<object> GetInvitationListByParticipantId(string participantId);
        Task<object> CreateVirtualClass(VClass vClassObj);
        Task<object> JoinVirtualClassByHost(VClassDetail vClassDetail, IEnumerable<ParticipantList> participantList);
        Task<object> JoinVirtualClassByParticipant(VClassDetail vClassDetail);
        Task<object> EndVirtualClassByHost(VClass vClassObj);
        Task<object> EndVirtualClassByParticipant(VClassDetail vClassDetail);
        Task<object> GetVirtualClassCallingHistoryByDaterange(DateTimeParams obj);
        Task<object> GetVirtualClassDetailById(long vclassId);
        Task<object> TestApi();

    }
}