using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebAPI.Data;
var builder = WebApplication.CreateBuilder(args);

// Add context
builder.Services.AddDbContext<WebAPIContext>();

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(x =>
    {
        x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddPolicy("Cors", builder =>
{
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
  .AddEntityFrameworkStores<WebAPIContext>()
  .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey =
                        JwtSecurityKey.Create(builder.Configuration["JWT:Secret"]),
                    ClockSkew = TimeSpan.Zero
                };
  });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Cors");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    RoleManager<IdentityRole> roleManager = 
        services.GetRequiredService<RoleManager<IdentityRole>>();

    UserManager<ApplicationUser> userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    CreateInitialRolesAndUsersAsync(userManager, roleManager)
        .Wait();
}

app.Run();

async Task CreateInitialRolesAndUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    try
    {
        string adminRoleName = Roles.Admin;
        if(!await roleManager.RoleExistsAsync(adminRoleName))
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));

        string staffRoleName = Roles.Staff;
        if(!await roleManager.RoleExistsAsync(staffRoleName))
            await roleManager.CreateAsync(new IdentityRole(staffRoleName));

        string studentRoleName = Roles.Student;
        if(!await roleManager.RoleExistsAsync(studentRoleName))
            await roleManager.CreateAsync(new IdentityRole(studentRoleName));

        var user = new ApplicationUser()
        {
            UserName = "admin@events.bolton.ac.uk",
            Email = "admin@events.bolton.ac.uk",
            Firstname = "Admin",
            Lastname = "User"
        };

        string password = "Super$ecretPassw0rd";

        if(await userManager.FindByNameAsync(user.UserName) == null)
        {
            var createSuccess =  await userManager.CreateAsync(user, password);
            if (createSuccess.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRoleName);
                await userManager.SetLockoutEnabledAsync(user, false);
            }
            else
            {
                throw new Exception(createSuccess.Errors.FirstOrDefault().ToString());
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception(ex.Message);
    }
}