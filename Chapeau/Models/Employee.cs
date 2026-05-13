using System.ComponentModel.DataAnnotations;

namespace Chapeau.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Naam is verplicht")]
        [StringLength(100, ErrorMessage = "Naam mag niet langer zijn dan 100 karakters")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        public string PasswordHash { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Selecteer een geldige rol")]
        public int RoleID { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
