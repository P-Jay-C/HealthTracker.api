using HealthTracker.api.Data;
using HealthTracker.api.Dtos.InComming;
using HealthTracker.api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("")] 
        public IActionResult GetUsers()
        {
            var Users = _context.Users.Where(x => x.status == 1).ToList();
            return Ok(Users);
        }

        [HttpPost]
        public IActionResult AddUsers( UserDto user )
        {
            User _user = new User();

            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
            _user.Country = user.Country;
            _user.Email = user.Email;
            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Phone = user.Phone;
            _user.status = 1;

            _context.Users.Add(_user);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser([FromQuery]Guid id)
        {
            var user = _context.Users.Where(x => x.Id == id);

            return Ok(user);
        }
    } 
}
