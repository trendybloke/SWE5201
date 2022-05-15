using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Data;
using WebAPI.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            _configuration = configuration;
        }

        private async Task<bool> AddOrRemoveRoleFromUserAsync
            (ApplicationUser user, string role, bool addUserToRole)
        {
            if (!roleManager.Roles.Select(r => r.Name).Contains(role))
                return false;

            var userRoles = await userManager.GetRolesAsync(user);

            var alreadyInRole = userRoles.Contains(role);

            bool success = false;

            if (addUserToRole && !alreadyInRole)
            {
                var result = await userManager.AddToRoleAsync(user, role);
                success = result.Succeeded;
            }
            else if (!addUserToRole && alreadyInRole)
            {
                var result = await userManager.RemoveFromRoleAsync(user, role);
                success = result.Succeeded;
            }
            else
                success = true;

            return success;
        }

        #region GET api/Accounts/List
        [HttpGet("List")]
        //[Authorize(Roles = Roles.Admin)]
        public ActionResult<IEnumerable<UserSummaryViewModel>> List()
        {
            var list = userManager
            .Users
            .OrderBy(u => u.Email)
            .Select(u => new UserSummaryViewModel()
            {
                Id = u.Id,
                Email = u.Email,
                Firstname = u.Firstname,
                Lastname = u.Lastname
            })
            .ToList();
            return list;
        }
        #endregion

        #region GET api/Accounts/Summary/{id}
        [HttpGet("Summary/{id}")]
        //[Authorize]
        public async Task<ActionResult<UserSummaryViewModel>> Summary(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return new UserSummaryViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
            };
        }
        #endregion

        #region GET api/Accounts/Modify/{id}
        [HttpGet("Modify/{id}")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<EditableUserViewModel>> Modify(string id)
        {
            if (id == null)
                return BadRequest();

            var user = await userManager.FindByIdAsync(id);
            if(user == null)
                return NotFound();

            var userRoles = await userManager.GetRolesAsync(user);

            return new EditableUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Admin = userRoles.Contains(Roles.Admin),
                Staff = userRoles.Contains(Roles.Staff),
                Student = userRoles.Contains(Roles.Student)
            };
        }
        #endregion

        #region POST api/Accounts/Register
        [HttpPost]
        [Route("Register")]
        //[AllowAnonymous]
        //[Authorize(Roles = Roles.Admin + "," + Roles.Staff)]
        public async Task<IActionResult> Register([FromBody] EditableUserViewModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Email);
            if (userExists != null)
                return Problem( "User already exists", 
                                null,
                                StatusCodes.Status500InternalServerError,
                                "Error",
                                nameof(ProblemDetails));

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email,
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return Problem( "User creation failed. Check details and try again.",
                                null,
                                StatusCodes.Status500InternalServerError,
                                "Error",
                                nameof(ProblemDetails));

            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Admin, model.Admin);
            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Staff, model.Staff);
            await this.AddOrRemoveRoleFromUserAsync(user, Roles.Student, model.Student);

            return Ok(new { Id = user.Id });
        }
        #endregion

        #region PUT api/Accounts/Modify/{id}
        [HttpPut("Modify/{id}")]
        //[Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Modify
            (string id, [FromBody] EditableUserViewModel modifiedUser)
        {
            try
            {
                if(id == null || id != modifiedUser.Id)
                    return Problem( "Id passed and Id in user do not match.",
                                    null,
                                    StatusCodes.Status500InternalServerError,
                                    "Error",
                                    nameof(ProblemDetails));

                var user = await userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                user.UserName = user.Email = modifiedUser.Email;
                user.Firstname = modifiedUser.Firstname;
                user.Lastname = modifiedUser.Lastname;

                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Admin, modifiedUser.Admin);
                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Staff, modifiedUser.Staff);
                await this.AddOrRemoveRoleFromUserAsync(user, Roles.Student, modifiedUser.Student);

                if (!string.IsNullOrEmpty(modifiedUser.Password))
                {
                    var resetPasswordToken = 
                        await userManager.GeneratePasswordResetTokenAsync(user);

                    await userManager.ResetPasswordAsync
                        (user, resetPasswordToken, modifiedUser.Password);
                }

                await userManager.UpdateAsync(user);
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }
        #endregion

        #region POST api/Accounts/Login
        [HttpPost]
        [Route("Login")]
        //[AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginUserViewModel model)
        {
            var user = await userManager.FindByNameAsync(model.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized();

            var userRoles = await userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.GivenName, user.Firstname),
                new Claim(JwtClaimTypes.FamilyName, user.Lastname),
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.Id, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach(var userRole in userRoles)
                authClaims.Add(new Claim(JwtClaimTypes.Role, userRole));

            var authSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(24),
                    claims: authClaims,
                    signingCredentials: 
                        new SigningCredentials
                            (authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var tokenViewModel = new TokenViewModel()
            {
                Data = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };

            return Ok(tokenViewModel);
        }
        #endregion
    }
}
