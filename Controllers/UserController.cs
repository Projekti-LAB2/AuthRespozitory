using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Context;
using AuthenticationAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthenticationAPI.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {

        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj) {
            if(userObj == null){
                return BadRequest();
            }
            var user = await _authContext.Users
                    .FirstOrDefaultAsync(x => x.UserName == userObj.UserName && x.Password == userObj.Password);
            if(user == null) {
                return NotFound(new {Message = "User not found!"});
            }
            return Ok(new {Message = "Logged in successfully!"});
        }
    
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj){
            if(userObj == null) {
                return BadRequest();
            }
            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();

            return Ok(new {Message = "User registered!"});
        }
    }
}