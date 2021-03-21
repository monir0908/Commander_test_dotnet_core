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
    public class MasterSettingController : ControllerBase
    {
        private IMasterSettingServices _services;

        public MasterSettingController(IMasterSettingServices services)
        {
            this._services = services;
        }

        // Project Related Endpoints

        [HttpGet, Route("GetProjectList")]
        public async Task<IActionResult> GetProjectList(int size, int pageNumber)
        {
            return Ok(await _services.GetProjectList(size,pageNumber));
        }

        [HttpGet, Route("GetProjectDetailById/{projectId:long}")]
        public async Task<IActionResult> GetProjectDetailById(long projectId)
        {
            return Ok(await _services.GetProjectDetailById(projectId));
        }

        [HttpPost, Route("CreateOrUpdateProject")]
        public async Task<IActionResult> CreateOrUpdateProject(Project model)
        {
            return Ok(await _services.CreateOrUpdateProject(model));
        }


        // Batch Related Endpoints

        [HttpGet, Route("GetBatchList")]
        public async Task<IActionResult> GetBatchList(int size, int pageNumber)
        {
            return Ok(await _services.GetBatchList(size,pageNumber));
        }

        [HttpGet, Route("GetBatchDetailById/{batchId:long}")]
        public async Task<IActionResult> GetBatchDetailById(long batchId)
        {
            return Ok(await _services.GetBatchDetailById(batchId));
        }        

        [HttpPost, Route("CreateOrUpdateBatch")]
        public async Task<IActionResult> CreateOrUpdateBatch(Batch model)
        {
            return Ok(await _services.CreateOrUpdateBatch(model));
        }


        //Project Batch Related Endpoints

        [HttpGet, Route("GetMergeableBatchListByProjectId/{projectId:long}")]
        public async Task<IActionResult> GetMergeableBatchListByProjectId(long projectId)
        {
            return Ok(await _services.GetMergeableBatchListByProjectId(projectId));
        }

        [HttpPost, Route("MergeProjectBatch")]
        public async Task<IActionResult> MergeProjectBatch(IEnumerable<ProjectBatch> models)
        {
            return Ok(await _services.MergeProjectBatch(models));
        }

        [HttpPost, Route("MergeProjectBatchHost")]
        public async Task<IActionResult> MergeProjectBatchHost(IEnumerable<ProjectBatchHost> models)
        {
            return Ok(await _services.MergeProjectBatchHost(models));
        }

        [HttpGet, Route("GetHostList")]
        public async Task<IActionResult> GetHostList(int size, int pageNumber)
        {
            return Ok(await _services.GetHostList(size,pageNumber));
        }

        [HttpGet, Route("GetMergeableHostList/{projectId:long}/{batchId:long}")]
        public async Task<IActionResult> GetMergeableHostList(long projectId, long batchId, int size, int pageNumber)
        {
            return Ok(await _services.GetMergeableHostList(projectId, batchId, size,pageNumber));
        }

        [HttpGet, Route("GetAlreadyMergeableHostList/{projectId:long}/{batchId:long}")]
        public async Task<IActionResult> GetAlreadyMergeableHostList(long projectId, long batchId, int size, int pageNumber)
        {
            return Ok(await _services.GetAlreadyMergeableHostList(projectId, batchId, size,pageNumber));
        }

        [HttpGet, Route("GetParticipantList")]
        public async Task<IActionResult> GetParticipantList(int size, int pageNumber)
        {
            return Ok(await _services.GetParticipantList(size,pageNumber));
        }


    }
}
