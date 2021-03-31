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
        public async Task<object> CreateOrUpdateHeadRole(HeadRoles model, IIdentity identity)
        {
            
            HeadRoles item = null;
            bool isEdit = true;
            try
            {
                if (await _context.HeadRoles.AnyAsync(x => x.Id != model.Id && x.Name == model.Name))
                    throw new Exception("Head Role Name already exists !");

                

                if (model.Id >0)
                {
                    item = await _context.HeadRoles.FindAsync(model.Id);
                    if (item == null) throw new Exception("Head Role not found to update !");
                }
                else
                {
                    item = new HeadRoles();
                    isEdit = false;                    
                }


               
                item.Name = model.Name;
                item.Description = model.Description;

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
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }


        

        public async Task<object> GetHeadRoleList(int size, int pageNumber)
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
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Description,
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

        public async Task<object> CreateRole(IdentityRole model)
        {
            
            try
            {

                //Need to rewrite this piece of code

                // foreach (var model in models)
                // {
                //     if (await _context.Roles.AnyAsync(x => x.Id != model.Id && x.Name == model.Name))
                //     throw new Exception("One or more roles already exit !");
                // }
                

                
                model.Id = Guid.NewGuid().ToString();               
                _context.Roles.Add(model);                

                await _context.SaveChangesAsync();             
                

                return new
                {
                    Success = true,
                    Message = "Successfully Saved",
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


        public async Task<object> GetRoleList()
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
        
        public async Task<object> GetRoleById(string id)
        {
            try
            {
                var data = await _context.Roles.Where(w => w.Id == id)
                .Select(t => new{
                    t.Id,
                    t.Name,
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


