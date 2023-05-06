using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Context;
using AuthenticationAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase {

        private readonly AppDbContext _authContext;
        public UserController(AddDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj) {
            if(userObj == null){
                return BadRequest();
            }
            var user = await _authContext.Users
                    .FirstOrDefaultAsync(x => x.Id == userObj.Id && x.Password == userObj.Password);
            if(user == null) {
                return NotFound(new {Message = "User not found!"});
            }
            return Ok(new {Message = "Logged in successfully!"})
        }
    }
}