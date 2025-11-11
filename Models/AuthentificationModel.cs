using System.ComponentModel.DataAnnotations;

namespace AppointmentApplication.Models
{
    public class AuthentificationModel
    {
        [Required]
        public string? Login { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
