using System.ComponentModel.DataAnnotations;

namespace Chapeau.ViewModels.Login
{
    /// <summary>
    /// View model for the login form, containing credentials and UI state.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>The employee's username.</summary>
        [Required(ErrorMessage = "Naam is verplicht")]
        [Display(Name = "Naam")]
        public string Name { get; set; } = string.Empty;

        /// <summary>The employee's password.</summary>
        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>Whether to persist the authentication cookie across browser sessions.</summary>
        public bool RememberMe { get; set; }

        /// <summary>Optional URL to redirect to after successful login.</summary>
        public string? ReturnUrl { get; set; }

        /// <summary>Error message displayed on the login form when authentication fails.</summary>
        public string? ErrorMessage { get; set; }
    }
}
