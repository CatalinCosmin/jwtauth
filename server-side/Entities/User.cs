using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace server_side.Entities
{
    public class User : UserDto
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
