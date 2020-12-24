using System.ComponentModel.DataAnnotations;

namespace backend_api.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
