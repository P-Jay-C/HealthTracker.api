using HealthTracker.api.Data;
using HealthTracker.api.Dtos.InComming;
using HealthTracker.api.Model;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.api.Controllers.v1
{
    
    public class UsersController : BaseController
    { 
        
        // private readonly AppDbContext _context;
        public UsersController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        // Get All 
        [HttpGet()]
        [HttpHead]
        public async Task<IActionResult> GetUsers()
        {
            // var Users = _context.Users.Where(x => x.status == 1).ToList();
            var users = await _unitOfWork.User.All();

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUsers(UserDto user)
        {
            User _user = new User();

            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
            _user.Country = user.Country;
            _user.Email = user.Email;
            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Phone = user.Phone;
            _user.status = 1;

            //_context.Users.Add(_user);
            //_context.SaveChanges();

            await _unitOfWork.User.Add(_user);
            await _unitOfWork.CompleteAsync();

            return CreatedAtRoute("GetUser", new { id = _user.Id }, _user);
        }

        [HttpGet]
        [Route("GetUser", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            //var user = _context.Users.Where(x => x.Id == id);

            var user = await _unitOfWork.User.GetById(id);

            return Ok(user);
        }

        
    }
}
