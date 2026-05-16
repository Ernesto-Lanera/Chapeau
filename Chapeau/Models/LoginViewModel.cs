using System.ComponentModel.DataAnnotations;

namespace Chapeau.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Employee name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}