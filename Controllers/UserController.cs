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
    public class UserController : ControllerBase
    {
        private IUserServices _services;

        public UserController(IUserServices services)
        {
            this._services = services;
        }

        // User Related Endpoints

        [HttpGet, Route("GetUserList")]
        public async Task<IActionResult> GetUserList(int size, int pageNumber)
        {
            return Ok(await _services.GetUserList(size, pageNumber));
        }
        


    }
}
