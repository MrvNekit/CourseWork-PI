namespace AppointmentApplication.Models
{
    public class WorkDayOptions
    {
        public const string Position = "WorkDay";

        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(8);
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(13);
        public TimeSpan AppointmentDuration { get; set; } = TimeSpan.FromMinutes(15);
    }
}
