using System.ComponentModel.DataAnnotations;

namespace AppointmentApplication.Models
{
    public class AppointmentDataModel
    {
        [Required]
        public string? Direction { get; set; }

        [Required]
        public string? Date { get; set; }
        public List<TimePartModel>? Timetables { get; set; }
        [Required]
        public string? SelectedNumber { get; set; }

        // Full Name
        [Required]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string? LastName { get; set; }

        // Passport
        [Required]
        public string? SNILS { get; set; }
        [Required]
        [RegularExpression("\\d{4}")]
        public string? PassportSeries { get; set; }
        [Required]
        [RegularExpression("\\d{6}")]
        public string? PassportNumber { get; set; }
        
        // Feedback
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^((8|\\+7)[\\- ]?)?(\\(?\\d{3}\\)?[\\- ]?)?[\\d\\- ]{7,10}$")]
        public string? PhoneNumber { get; set; }
    }
}
