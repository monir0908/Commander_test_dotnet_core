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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;
using System.Transactions;

namespace Commander.Services{

    

    public class RoleServices : IRoleServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SignalHub> _notificationHubContext;

        public RoleServices(ApplicationDbContext context, IHubContext<SignalHub> hubContext)
        {
            this._context = context;
            this._notificationHubContext = hubContext;
        }



        public async Task<object> HeadRolesCreateOrUpdate(HeadRolesModel model, IIdentity identity)
        {
            HeadRoles item = null;
            HeadRoles_Roles role = null;
            List<HeadRoles_Roles> roles = new List<HeadRoles_Roles>();
            bool isEdit = true;
            try
            {
                if (await _context.HeadRoles.AnyAsync(x => x.Id != model.Id && x.Name == model.Name))
                    throw new Exception("Already exists: " + model.Name);

                if (model.Id > 0)
                {
                    item = await _context.HeadRoles.FindAsync(model.Id);
                    if (item == null) throw new Exception("Role group not found");

                    roles = await _context.HeadRoles_Roles.Where(x => x.HeadRoleId == item.Id).ToListAsync();
                }
                else
                {
                    item = new HeadRoles();
                    isEdit = false;
                }

                item.Name = model.Name;
                item.Description = model.Description;
                item.NoOfRoles = model.Roles.Count;


                var identityRoles = await _context.Roles.Where(x => model.Roles.Contains(x.Name)).ToListAsync();

                foreach (var rolename in model.Roles)
                {
                    role = roles.FirstOrDefault();
                    if (role == null) role = new HeadRoles_Roles();
                    role.HeadRoleId = item.Id;
                    role.RoleId = identityRoles.Find(x => x.Name == rolename).Id;

                    if (role.Id == 0)
                    {
                        _context.HeadRoles_Roles.Add(role);
                    }
                    else
                    {
                        _context.Entry(role).State = EntityState.Modified;
                        roles.RemoveAt(0);
                    }
                }

                foreach (var HeadRoles_Roles in roles)
                {
                    _context.Entry(HeadRoles_Roles).State = EntityState.Deleted;
                }

                item.SetAuditTrailEntity(identity);

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        if (!isEdit)
                        {
                            _context.HeadRoles.Add(item);
                        }
                        else
                        {
                            _context.Entry(item).State = EntityState.Modified;

                            //var userIds = await _context.BPUsers.Where(x => x.HeadRoleId == item.Id && x.UserId != null).Select(x => x.UserId).ToListAsync();

                            //foreach (var userId in userIds)
                            //{
                            //    userRoles = await usermanager.GetRolesAsync(userId);
                            //    result = await usermanager.RemoveFromRolesAsync(userId, userRoles.ToArray());
                            //    if (!result.Succeeded) throw new Exception("Error on user's permissions remove: " + string.Join(", ", result.Errors));

                            //    result = await usermanager.AddToRolesAsync(userId, model.Roles.ToArray());
                            //    if (!result.Succeeded) throw new Exception("Error on user's new roles add: " + string.Join(", ", result.Errors));
                            //}
                        }
                        await _context.SaveChangesAsync();
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw (ex);
                    }

                }

                return new
                {
                    Success = true,
                    Message = "Successfully " + (isEdit ? "Updated" : "Saved"),
                    item.Id
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

        public async Task<object> GetHeadRolesList(int size, int pageNumber)
        {
            try
            {
                // var data = await _context.HeadRoles.OrderByDescending(x => x.Name)
                //     .GroupJoin(_context.HeadRoles_Roles, x => x.Id, y => y.HeadRoleId, (x, y) => new
                //     {
                //         HeadRoles = x,
                //         Roles = y
                //     }).Select(x => new { x.HeadRoles.Id, x.HeadRoles.Name, x.HeadRoles.Description, x.HeadRoles.NoOfRoles, Roles = x.Roles.Select(y => y.Id).ToList() }).Skip(pageNumber * size).Take(size).ToListAsync();
                
                // HeadRoles, HeadRoles_Roles, Roles
                var data = 
                _context.HeadRoles
                .Select( hr => new{
                    hr.Id,
                    hr.Name,
                    hr.Description,
                    hr.NoOfRoles,

                    Roles = _context.HeadRoles_Roles
                    .Where(x => x. HeadRoleId == hr.Id)
                    .Join(_context.Roles,
                    x => x.RoleId,
                    y => y.Id,
                    (x, y)=> new {HeadRoles = x, Roles = y})
                    .Select(x => x.Roles.Name).ToList(),

                })
                .Skip(pageNumber * size)
                .Take(size)
                .ToList();
            
            
                
                var count = await _context.HeadRoles.CountAsync();

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
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }

        public async Task<object> GetHeadRolesById(long id)
        {
            try
            {
                var data = await _context.HeadRoles.Where(t => t.Id == id)
                     .GroupJoin(_context.HeadRoles_Roles, x => x.Id, y => y.HeadRoleId, (x, y) => new
                     {
                         HeadRoles = x,
                         Roles = y
                     })
                .Select(t => new
                {
                    t.HeadRoles.Id,
                    t.HeadRoles.Name,
                    t.HeadRoles.Description,
                    Roles = t.Roles.Select(x => x.Id).ToList()
                }).FirstOrDefaultAsync();
                return new
                {
                    Success = true,
                    Record = data
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

        public async Task<object> GetHeadRolesDropDownList()
        {
            try
            {
                var data = await _context.HeadRoles.OrderBy(o => o.Name)
                .Select(t => new
                {
                    t.Id,
                    t.Name
                }).ToListAsync();

                return new
                {
                    Success = true,
                    Records = data
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

        public async Task<object> GetAllRoles()
        {
            try
            {
                var list = await _context.Roles.OrderBy(o => o.Name)
                .Select(t => t.Name).ToListAsync();

                var data = list.Select(x => new { Id = x, Name = x.Replace("_", " ") }).ToList();

                return new
                {
                    Success = true,
                    Records = data
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

        public async Task<object> GetRolesByHeadId(long id)
        {
            try
            {
                var data = await _context.HeadRoles_Roles.Where(w => w.HeadRoleId == id)
                .Select(t => t.RoleId).ToListAsync();

                return new
                {
                    Success = true,
                    Records = data
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
        
        
    }
}


