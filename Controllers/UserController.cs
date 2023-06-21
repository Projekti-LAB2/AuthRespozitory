using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Context;
using AuthenticationAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AngularAuthAPI.Helpers;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

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
                    .FirstOrDefaultAsync(x => x.UserName == userObj.UserName);
            if(user == null) {
                return NotFound(new {Message = "User not found!"});
            }
            if(!PasswordHasher.VerifyPassword(userObj.Password, user.Password)) 
            { 
                return BadRequest(new {Message = "Password is incorrect!"});
            }

            user.Token = CreateJwt(user);

            return Ok(new {
                Token = user.Token,
                Message = "Logged in successfully!"
            });
        }
    
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj){
            if(userObj == null) {
                return BadRequest();
            }
            //Check userName
            if(await CheckUserNameExistsAsync(userObj.UserName))
            {
                return BadRequest(new {Message = "User name exists!"});
            }
            // Check Email
            if (await CheckEmailExistsAsync(userObj.Email))
            {
                return BadRequest(new { Message = "Email exists!" });
            }
            //Check password
            var pass = CheckPasswordStrength(userObj.Password);
            
            if(!string.IsNullOrEmpty(pass))
            {
                return BadRequest(new { Message = pass.ToString() });
            }

            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "Admin";
            userObj.Token = "";
            await _authContext.Users.AddAsync(userObj);
            await _authContext.SaveChangesAsync();

            return Ok(new {Message = "User registered!"});
        }

        private async Task<bool> CheckUserNameExistsAsync(string userName)
        {
            return await _authContext.Users.AnyAsync(x => x.UserName == userName);
        }
        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _authContext.Users.AnyAsync(x => x.Email == email);
        }
        private string CheckPasswordStrength(string password) 
        {
            StringBuilder sb = new StringBuilder();
            if(password.Length < 6) {
                sb.Append("Minimum password length should be 6" + Environment.NewLine);
            }
            if(!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]"))) 
            {
                sb.Append("Password should be Alphanumeric" + Environment.NewLine);
            }
            if(!Regex.IsMatch(password, "[<,>,?,.,/,;,:,',\\[,\\],{,},|,\\,!,@,#,$,%,^,&,*,(,),_,-,+,=]"))
            {
                sb.Append(" Password should contain at least one symbol" + Environment.NewLine);
            }
            return sb.ToString();
        }
        private string CreateJwt(User user) 
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysecret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _authContext.Users.ToListAsync());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            return Ok(await _authContext.Users.FindAsync(id));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _authContext.Users.Remove(user);
            await _authContext.SaveChangesAsync();

            return Ok(user);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, User updatedUser)
        {
           

            var user = await _authContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update the properties of the user entity with the updatedUser data
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.UserName = updatedUser.UserName;
            user.Role = updatedUser.Role;
            user.Password = user.Password;

            // Save the changes to the database
            await _authContext.SaveChangesAsync();

            return Ok(user);
        }

    }
}