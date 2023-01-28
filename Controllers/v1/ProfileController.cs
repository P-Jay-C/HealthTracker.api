using HealthTracker.api.Data;
using HealthTracker.api.Dtos.Generic;
using HealthTracker.api.Dtos.InComming.Profile;
using HealthTracker.api.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.api.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : BaseController
    {
        public ProfileController(IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager) : base(unitOfWork, userManager)
        {}

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
            var result = new Result<User>();

            if (loggedInUser == null)
            {   
                result.Error = new Dtos.Errors.Error()
                {
                    Code = 400,
                    Message = "User not found",
                    Type = "Bad Request"
                };

                return BadRequest(result);
            }


            var identityId = new Guid(loggedInUser.Id);

      
            var profile = await _unitOfWork.User.GetByIdentityId(identityId);

            if(profile == null)
            {
                result.Error = new Dtos.Errors.Error()
                {
                    Code = 400,
                    Message = "User not found",
                    Type = "Bad Request"
                };

                return BadRequest(result);
            }

            result.Content = profile;
            return Ok(profile);
        }

        [HttpPut] 
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfile)
        {
            // If model is valid

            if (!ModelState.IsValid) 
                return BadRequest("Invalid payload");

            var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

            if (loggedInUser == null)
            {
                return BadRequest("User not found");
            }


            var identityId = new Guid(loggedInUser.Id);
            var userProfile = await _unitOfWork.User.GetByIdentityId(identityId);

            if (userProfile == null)
                return BadRequest("User not found");

            userProfile.Country = updateProfile.Country;
            userProfile.Address = updateProfile.Address;
            userProfile.MobileNumber = updateProfile.MobileNumber;
            userProfile.Sex = updateProfile.Sex;

            var isUpdated = await _unitOfWork.User.UpdateUserProfile(userProfile);

            if (isUpdated)
            {
                await _unitOfWork.CompleteAsync();
                return Ok(userProfile);
            }

            return BadRequest("Something went wrong please try again later");
        }

        
    }



}
