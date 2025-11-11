using Microsoft.EntityFrameworkCore;
using AppointmentApplication.Models;

namespace AppointmentApplication.DatabaseContexts
{
    public class AccountContext : DbContext
    {
        public AccountContext() { }
        public AccountContext(DbContextOptions<AccountContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<AccountModel> Accounts { get; set; } = default!;
    }
}
