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

namespace HealthTracker.api.Controllers.v1;

public class AccountsController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtConfig _jwtConfig;


    public AccountsController(
        IUnitOfWork unitOfWork,
        UserManager<IdentityUser> userManager,
        TokenValidationParameters tokenValidationParameters,
        IOptionsMonitor<JwtConfig> optionsMonitor) : base(unitOfWork, userManager)
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

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
    {
        if (ModelState.IsValid)
        {
            // check if result is valid
            var result = await VerifyToken(tokenRequestDto);

            if (result == null)
            {
                return BadRequest(new UserLoginResponseDto()
                {
                    Success = false,
                    Errors = new List<string>(){
                "token validation failed"
            }
                });
            }
            return Ok(result);
        }

        return null;
    }

    private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Check the validity of token
            var principal = tokenHandler.ValidateToken(tokenRequestDto.Token, _tokenValidationParameters, out var validateToken);

            if (validateToken is JwtSecurityToken jwtSecurityToken)
            {
                // checking if same security algorithm is used
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                if (!result)
                    return null;
            }

            // Check ExpiryDate of Token
            var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            // Convert to date to check

            var expDate = (DateTime)UnixTimeStampToDate(utcExpiryDate);

            if (expDate > DateTime.UtcNow)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Jwt Token has not expired"
                }

                };
            }

            // Check if the refresh token exists
            var refreshTokenExist = await _unitOfWork.RefreshToken.GetByRefreshTokenAsync(tokenRequestDto.RefreshToken);

            if (refreshTokenExist == null)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Invalid refresh token"
                }

                };
            }

            if (refreshTokenExist.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Refresh token has expired, please login again"
                }

                };
            }

            // check if refresh token has be used

            if (refreshTokenExist.IsUsed)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Refresh token has been used"
                }

                };
            }

            // check if refresh token has been revoked

            if (refreshTokenExist.IsRevoked)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Refresh token has been revoked, cannot be used."
                }

                };
            }

            var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (refreshTokenExist.JwtId != jti)
            {
                return new AuthResult()
                {
                    Success = false,
                    Errors = new List<string>()
                {
                    "Refresh token reference does not match the jwt token."
                }

                };
            }

            // start processing and get a new token

            refreshTokenExist.IsUsed = true;
            var updateResult = await _unitOfWork.RefreshToken.MarkRefreshTokenAsUsedAsync(refreshTokenExist);
            if (updateResult)
            {
                await _unitOfWork.CompleteAsync();

                var dbUser = await _userManager.FindByIdAsync(refreshTokenExist.UserId);

                if (dbUser == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                     {
                    "Error processing request"
                    }
                    };
                }

                // generate a jwt token

                var tokens = await GenerateJwtToken(dbUser);
                return new AuthResult
                {
                    Token = tokens.JwTToken,
                    Success = true,
                    RefreshToken = tokens.RefreshToken

                };
            }

            return new AuthResult()
            {
                Success = false,
                Errors = new List<string>()
                {
                    "Error processing request."
                }

            };


        }
        catch (Exception)
        {
            return null;
        }

    }

    private object UnixTimeStampToDate(long unixDate)
    {
        // Sets the time to 1,Jan, 1970
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();

        return dateTime;
    }

    private async Task<TokenData> GenerateJwtToken(IdentityUser user)
    {


        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim("id", user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

        }),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryDateFrame)
        };

        var jwtHandler = new JwtSecurityTokenHandler();
        // Generate the security obj token
        var token = jwtHandler.CreateToken(tokenDescriptor);

        // Convert the security obj token into a string
        var jwtToken = jwtHandler.WriteToken(token);

        // Generate a refresh token
        var refreshToken = new RefreshToken
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