using System.ComponentModel.DataAnnotations;

namespace AppointmentApplication.Models
{
    public class AccountModel
    {
        public int Id { get; set; }

        // Full Name
        [Required]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string? LastName { get; set; }

        // Roles
        public string Role {  get; set; } = string.Empty;
        public List<string>? Speciality { get; set; }

        // Authentification data
        [Required]
        public string? Login { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
