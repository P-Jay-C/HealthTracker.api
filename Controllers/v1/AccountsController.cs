using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthTracker.api.Configuration;
using HealthTracker.api.Data;
using HealthTracker.api.Dtos.Generic;
using HealthTracker.api.Dtos.InComming;
using HealthTracker.api.Dtos.OutGoing;
using HealthTracker.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthTracker.api.Controllers.v1
{
    public class AccountsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;


        public AccountsController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            TokenValidationParameters tokenValidationParameters,
            IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userRegistrationDto)
        {
            // Check if the model is valid
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var userExists = await _userManager.FindByEmailAsync(userRegistrationDto.Email);


                if (userExists != null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Email already in use"
                        }
                    });
                }


                // Add User

                var newUser = new IdentityUser()
                {
                    Email = userRegistrationDto.Email,
                    UserName = userRegistrationDto.Email,
                    EmailConfirmed = true
                };

                var isCreated = await _userManager.CreateAsync(newUser, userRegistrationDto.Password);

                if (!isCreated.Succeeded)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }


                User _user = new User();
                _user.IdentityId = new Guid(newUser.Id);
                _user.DateOfBirth = DateTime.Now; //Convert.ToDateTime(newUser.DateOfBirth);
                _user.Country = "";
                _user.Email = userRegistrationDto.Email;
                _user.FirstName = userRegistrationDto.FirstName;
                _user.LastName = userRegistrationDto.LastName;
                _user.Phone = "";
                _user.status = 1;


                await _unitOfWork.User.Add(_user);
                await _unitOfWork.CompleteAsync();

                // Create a jwt token
                var token = await GenerateJwtToken(newUser);

                // return back to the user

                return Ok(new UserRegistrationResponseDto()
                {
                    Success = true,
                    Token = token.JwTToken,
                    RefreshToken = token.RefreshToken
                });

            }
            else
            {
                return BadRequest(new UserRegistrationResponseDto()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto LoginDto)
        {
            if (ModelState.IsValid)
            {

                //Check if user exits
                var userExits = await _userManager.FindByEmailAsync(LoginDto.Email);

                if (userExits == null)
                {
                    return BadRequest(new UserRegistrationResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Authentication request"
                        }
                    });
                }

                // Check if password is correct

                var isCorrect = await _userManager.CheckPasswordAsync(userExits, LoginDto.Password);

                if (isCorrect)
                {
                    var jwtToken = await GenerateJwtToken(userExits);

                    return Ok(new UserLoginResponseDto()
                    {
                        Success = true,
                        Token = jwtToken.JwTToken,
                        RefreshToken = jwtToken.RefreshToken

                    });

                }
                else
                {
                    return BadRequest(new UserLoginResponseDto()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Password does not match."
                        }
                    });
                }
            }
            else
            { 
                return BadRequest(new UserLoginResponseDto()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid Payload"
                    }
                });
            }

        }


        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

                }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryDateFrame)
            };

            // Generate the security obj token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            // Convert the security obj token into a string
            var jwtToken = jwtHandler.WriteToken(token);

            // Generate a refresh token
            var refreshToken = new RefreshToken()
            {
                AddedDate = DateTime.UtcNow,
                Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                status = 1,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };


            await _unitOfWork.RefreshToken.Add(refreshToken);
            await _unitOfWork.CompleteAsync();

            var tokenData = new TokenData()
            {
                JwTToken = jwtToken,
                RefreshToken = refreshToken.Token 
            };


            return tokenData;
        }

        private string RandomStringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
