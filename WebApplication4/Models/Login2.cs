using System.ComponentModel.DataAnnotations;
namespace WebApplication4.Models
{
public class Login2
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool Remember { get; set; }
    }

}
