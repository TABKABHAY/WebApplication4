using System.ComponentModel.DataAnnotations;
namespace WebApplication4.Models
{
public class Login2
    {
        [Required]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        public bool remember { get; set; }
    }

}
