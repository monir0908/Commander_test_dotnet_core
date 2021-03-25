using System.Text;
using System.Text.Json;
using Commander.Models;
using Commander.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Commander
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    // need to add the following as JObject is used in controller                    
                    .AddNewtonsoftJson(options =>
                    {
                        options.UseMemberCasing(); // Important for camel casing !
                    });

            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Cn")));

            // For Identity  
            services.AddIdentity<ApplicationUser, IdentityRole>()  
                .AddEntityFrameworkStores<ApplicationDbContext>()  
                .AddDefaultTokenProviders();  
  
            // Adding Authentication  
            services.AddAuthentication(options =>  
            {  
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;  
            })  
  
            // Adding Jwt Bearer  
            .AddJwtBearer(options =>  
            {  
                options.SaveToken = true;  
                options.RequireHttpsMetadata = false;  
                options.TokenValidationParameters = new TokenValidationParameters()  
                {  
                    ValidateIssuer = true,  
                    ValidateAudience = true,  
                    ValidAudience = Configuration["JWT:ValidAudience"],  
                    ValidIssuer = Configuration["JWT:ValidIssuer"],  
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))  
                };  
            }); 
                

            services.AddMvc()
            .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services
            .AddScoped<IConferenceServices, ConferenceServices>()
            .AddScoped<IMasterSettingServices, MasterSettingServices>()
            .AddScoped<IUserServices, UserServices>()
            .AddScoped<IRoleServices, RoleServices>()
            ;

            
            services.AddSignalR();


            services.AddCors(option =>
            {
                option.AddPolicy("CorsPolicy", builder =>
                         builder.WithOrigins("*", "http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Commander", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Commander v1"));
            }


            
            

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors("CorsPolicy");
            

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SignalHub>("/pushNotification");
            });
        }
    }
}
