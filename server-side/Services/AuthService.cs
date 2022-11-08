using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using server_side.Entities;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace server_side.Services
{
public class AuthService : IAuthService
{

        private readonly UserContext _userContext;

        public AuthService(UserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task<string?> ValidateUser(UserDto userDto)
        {
            User? user = await _userContext.Users.SingleOrDefaultAsync(x => x.Username == userDto.Username);

            if (user == null)
            {
                return null;
            }

            if(!BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
            {
                return null;
            }

            var jwtToken = CreateJwtToken(user);

            return jwtToken;
        }
        private string CreateJwtToken(User user)
        {
            var builder = Program.appBuilder;

            var issuer = builder.Configuration["JWT:Issuer"];
            var audience = builder.Configuration["JWT:Audience"];
            var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }
        public async Task<bool> RegisterUser(UserDto userDto)
        {
            User? user = await _userContext.Users.SingleOrDefaultAsync(x => x.Username == userDto.Username);
            if (user != null)
            {
                return false;
            }

            user = new User();
            user.Id = Guid.NewGuid();
            user.Username = userDto.Username;
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            _userContext.Users.Add(user);
            await _userContext.SaveChangesAsync();

            return true;
        }
    }
}
