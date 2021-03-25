using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commander.Models;
using Microsoft.EntityFrameworkCore;
using Commander.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;

namespace Commander.Services{


    public interface IRoleServices
    {
        // Role Interfaces
        Task<object> HeadRolesCreateOrUpdate(HeadRolesModel model, IIdentity identity);
        Task<object> GetHeadRolesList(int size, int pageNumber);
        Task<object> GetHeadRolesById(long id);
        Task<object> GetHeadRolesDropDownList();
        Task<object> GetAllRoles();
        Task<object> GetRolesByHeadId(long id);



    }
}