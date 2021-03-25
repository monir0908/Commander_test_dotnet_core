using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Commander.Services;
using Commander.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace Commander.Controllers
{
    [Route("api/[controller]")]
    
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class RoleController : ControllerBase
    {
        private IRoleServices _services;

        public RoleController(IRoleServices services)
        {
            this._services = services;
        }

        [HttpPost, Route("create-or-update")]
        public async Task<IActionResult> HeadRolesCreateOrUpdate(HeadRolesModel model)
        {
            return Ok(await _services.HeadRolesCreateOrUpdate(model, User.Identity));
        }

        [HttpGet, Route("list")]
        public async Task<IActionResult> GetHeadRolesList(int size, int pageNumber)
        {
            return Ok(await _services.GetHeadRolesList(size, pageNumber));
        }

        [HttpGet, Route("get/{id}")]
        public async Task<IActionResult> GetHeadRolesById(long id)
        {
            return Ok(await _services.GetHeadRolesById(id));
        }

        [HttpGet, Route("dropdown-list")]
        public async Task<IActionResult> GetHeadRolesDropDownList()
        {
            return Ok(await _services.GetHeadRolesDropDownList());
        }
        

        [HttpGet, Route("permission/list")]
        public async Task<IActionResult> GetAllRoles()
        {
            return Ok(await _services.GetAllRoles());
        }

        [HttpGet, Route("{id}/permissions")]
        public async Task<IActionResult> GetRolesByHeadId(long id)
        {
            return Ok(await _services.GetRolesByHeadId(id));
        }

        
        
        


    }
}
