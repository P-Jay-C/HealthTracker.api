using HealthTracker.api.Data;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.api.Controllers.v1
{
    [Route("api/v{Version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController:ControllerBase
    {
        public IUnitOfWork _unitOfWork;

        public BaseController(IUnitOfWork unitOfWork)//AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            // _context = context;
        }
    }
}
