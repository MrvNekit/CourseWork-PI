using System.ComponentModel.DataAnnotations;

namespace AppointmentApplication.Models
{
    public class ScheduleSelection
    {
        [Required]
        public string? Date { get; set; }
        public string? Direction { get; set; }

    }
}
