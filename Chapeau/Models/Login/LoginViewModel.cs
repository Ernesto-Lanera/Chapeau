using System.ComponentModel.DataAnnotations;

namespace Chapeau.Models.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Naam is verplicht")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
