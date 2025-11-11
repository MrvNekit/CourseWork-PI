using Microsoft.EntityFrameworkCore;
using AppointmentApplication.Models;

namespace AppointmentApplication.DatabaseContexts
{
    public class AppointmentContext : DbContext
    {
        public AppointmentContext(DbContextOptions<AppointmentContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<AppointmentModel> Appointments { get; set; } = default!;
    }
}
