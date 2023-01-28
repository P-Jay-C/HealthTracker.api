using HealthTracker.api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.api.Controllers.v1
{
    [Route("api/v{Version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController:ControllerBase
    {
        public IUnitOfWork _unitOfWork;
        public  UserManager<IdentityUser> _userManager;

        public BaseController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)//AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            // _context = context;
        }
    }
}
