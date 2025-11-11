namespace AppointmentApplication.Models
{
    public class TimePartModel
    {
        public string Time = string.Empty;
        public List<Fullname> Users = new List<Fullname>();
    }

    public struct Fullname
    {
        public string? FirstName;
        public string? MiddleName;
        public string? LastName;
        public int Id;
    }
}
