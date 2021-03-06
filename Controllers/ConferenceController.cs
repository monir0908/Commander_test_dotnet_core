using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Commander.Services;
using Commander.Models;

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

        [HttpPost, Route("EndConference")]
        public async Task<IActionResult> EndConference(Conference confObj)
        {
            return Ok(await _services.EndConference(confObj));
        }

        [HttpGet, Route("GetConferenceList")]
        public async Task<IActionResult> GetConferenceList()
        {
            return Ok(await _services.GetConferenceList());
        }


    }
}
