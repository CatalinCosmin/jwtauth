using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using server_side.Entities;
using server_side.Services;

namespace server_side.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IAuthService _authService;

        public AuthController(UserContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<string?>> GetUser()
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);
            Console.WriteLine(accessToken);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Program.appBuilder.Configuration["JWT:Key"]);

            tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var aux = jwtToken.Claims.First(x => x.Type == "Id").ToString().Replace("Id: ", string.Empty);

            var userId = Guid.Parse(aux);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            Console.WriteLine(userId);

            return Ok(user.Username);
        }


        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost("/add")]
        //public async Task<ActionResult<UserDto>> PostUser(UserDto userDto)
        //{
        //    userDto.Id = Guid.NewGuid().ToString();
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetUser", new { id = user.Id }, user);
        //}
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            var userRegistered = await _authService.RegisterUser(userDto);

            if(userRegistered == false)
            {
                return BadRequest();
            }

            return CreatedAtAction("Register", userDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            var jwtToken = await _authService.ValidateUser(userDto);
            if (jwtToken == null)
                return Unauthorized();
            else
            {
                return Ok(jwtToken);
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
