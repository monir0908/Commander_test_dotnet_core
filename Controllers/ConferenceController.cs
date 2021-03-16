using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Commander.Services;
using Commander.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Commander.Controllers
{
    [Route("api/[controller]")]
    
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class ConferenceController : ControllerBase
    {
        private IConferenceServices _services;

        public ConferenceController(IConferenceServices services)
        {
            this._services = services;
        }

        // Host Side


        [HttpGet, Route("GetProjectListByHostId/{hostId}")]
        public async Task<IActionResult> GetProjectListByHostId(string hostId)
        {
            return Ok(await _services.GetProjectListByHostId(hostId));
        }

        [HttpGet, Route("GetBatchListByProjectId/{pId:long}")]
        public async Task<IActionResult> GetBatchListByProjectId(long pId)
        {
            return Ok(await _services.GetBatchListByProjectId(pId));
        }

        [HttpGet, Route("GetParticipantListByBatchId/{batchId:long}")]
        public async Task<IActionResult> GetParticipantListByBatchId(long batchId)
        {
            return Ok(await _services.GetParticipantListByBatchId(batchId));
        }



        [HttpGet, Route("GetParticipantListByHostId/{hostId}")]
        public async Task<IActionResult> GetParticipantListByHostId(string hostId)
        {
            return Ok(await _services.GetParticipantListByHostId(hostId));
        }

        
        [HttpGet, Route("GetOnGoingConferenceByHostId/{hostId}")]
        public async Task<IActionResult> GetOnGoingConferenceByHostId(string hostId)
        {
            return Ok(await _services.GetOnGoingConferenceByHostId(hostId));
        }


        // Participant Side

        [HttpGet, Route("GetHostListByParticipantId/{pId}")]
        public async Task<IActionResult> GetHostListByParticipantId(string pId)
        {
            return Ok(await _services.GetHostListByParticipantId(pId));
        }








        [HttpPost, Route("CreateConference")]
        public async Task<IActionResult> CreateConference(Conference confObj)
        {
            return Ok(await _services.CreateConference(confObj));
        }
        
        
        [HttpPost, Route("JoinConferenceByHost")]
        public async Task<IActionResult> JoinConferenceByHost(Conference confObj)
        {
            return Ok(await _services.JoinConferenceByHost(confObj));
        }
        
        [HttpPost, Route("JoinConferenceByParticipant")]
        public async Task<IActionResult> JoinConferenceByParticipant(Conference confObj)
        {
            return Ok(await _services.JoinConferenceByParticipant(confObj));
        }

        [HttpPost, Route("EndConference")]
        public async Task<IActionResult> EndConference(Conference confObj)
        {
            return Ok(await _services.EndConference(confObj));
        }
        

        [HttpPost, Route("EndConferenceByParticipant")]
        public async Task<IActionResult> EndCEndConferenceByParticipantonference(Conference confObj)
        {
            return Ok(await _services.EndConferenceByParticipant(confObj));
        }

        [HttpGet, Route("GetConferenceList")]
        public async Task<IActionResult> GetConferenceList()
        {
            return Ok(await _services.GetConferenceList());
        }

        [HttpGet, Route("TestApi")]
        public async Task<IActionResult> TestApi()
        {
            return Ok(await _services.TestApi());
        }

        [HttpPost, Route("GetCallingHistoryByDaterange")]
        public async Task<IActionResult> GetCallingHistoryByDaterange(DateTimeParams obj)
        {
            return Ok(await _services.GetCallingHistoryByDaterange(obj));
        }

        [HttpGet, Route("GetConferenceHistoryDetailById/{confId}")]
        public async Task<IActionResult> GetConferenceHistoryDetailById(long confId)
        {
            return Ok(await _services.GetConferenceHistoryDetailById(confId));
        }






        [HttpGet, Route("GetParticipantListByBatchAndHostId/{batchId:long}/{hostId}")]
        public async Task<IActionResult> GetParticipantListByBatchAndHostId(long batchId, string hostId)
        {
            return Ok(await _services.GetParticipantListByBatchAndHostId(batchId, hostId));
        }

        [HttpGet, Route("GetCurrentOnGoingVirtualClassListByHostId/{hostId}")]
        public async Task<IActionResult> GetCurrentOnGoingVirtualClassListByHostId(string hostId)
        {
            return Ok(await _services.GetCurrentOnGoingVirtualClassListByHostId(hostId));
        }

        [HttpPost, Route("CreateVirtualClass")]
        public async Task<IActionResult> CreateVirtualClass(VClass vClassObj)
        {
            return Ok(await _services.CreateVirtualClass(vClassObj));
        }

        [HttpPost, Route("JoinVirtualClassByHost")]
        public async Task<IActionResult> JoinVirtualClassByHost(JObject objData)
        {

            dynamic jsonData = objData;
            JObject vClassDetailJson = jsonData.vClassDetail;
            var vClassDetail = vClassDetailJson.ToObject<VClassDetail>();

            JArray participantListJson = jsonData.participantList;
            var participantList = participantListJson.Select(item => item.ToObject<ParticipantList>()).ToList();
            return Ok(await _services.JoinVirtualClassByHost(vClassDetail, participantList));
        }

        [HttpPost, Route("JoinVirtualClasByParticipant")]
        public async Task<IActionResult> JoinVirtualClasByParticipant(VClassDetail vClassDetail)
        {
            return Ok(await _services.JoinVirtualClasByParticipant(vClassDetail));
        }
        

        [HttpPost, Route("EndVirtualClassByHost")]
        public async Task<IActionResult> EndVirtualClassByHost(VClass vClassObj)
        {
            return Ok(await _services.EndVirtualClassByHost(vClassObj));
        }

        [HttpPost, Route("EndVirtualClassByParticipant")]
        public async Task<IActionResult> EndVirtualClassByParticipant(VClassDetail vClassDetail)
        {
            return Ok(await _services.EndVirtualClassByParticipant(vClassDetail));
        }

        [HttpGet, Route("GetVirtualClassCallingHistoryByDaterange")]
        public async Task<IActionResult> GetVirtualClassCallingHistoryByDaterange(DateTimeParams obj)
        {
            return Ok(await _services.GetVirtualClassCallingHistoryByDaterange(obj));
        }

        [HttpGet, Route("GetVirtualClassDetailById/{vclassId}")]
        public async Task<IActionResult> GetVirtualClassDetailById(long vclassId)
        {
            return Ok(await _services.GetVirtualClassDetailById(vclassId));
        }


    }
}
