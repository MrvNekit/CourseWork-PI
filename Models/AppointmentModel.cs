using System.ComponentModel.DataAnnotations;

namespace AppointmentApplication.Models
{
    public class AppointmentModel
    {
        public int Id { get; set; }

        [Required]
        public int SpecialistId { get; set; }

        [Required]
        public string? Date { get; set; }
        [Required]
        public string? StartTime { get; set; }
        [Required]
        public string? EndTime { get; set; }

        [Required]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? SNILS { get; set; }
        [Required]
        public string? PassportSeries { get; set; }
        [Required]
        public string? PassportNumber { get; set; }

        [Required]
        public string? PhoneNumber { get; set; }
    }
}
