using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using AppointmentApplication.DatabaseContexts;

var builder = WebApplication.CreateBuilder(args);

string? databaseLocation = builder.Configuration.GetConnectionString("DatabaseLocation");
string? accountContextConnectionString = builder.Configuration.GetConnectionString("AccountContext");
string? appointmentContextConnectionString = builder.Configuration.GetConnectionString("AppointmentContext");

// Ensure that folder is created
if (databaseLocation != null)
{
    if(!Directory.Exists(databaseLocation))
        Directory.CreateDirectory(databaseLocation);
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
// SQLite Databases
builder.Services.AddDbContext<AccountContext>(options => options.UseSqlite("Data Source=" + databaseLocation + accountContextConnectionString));
builder.Services.AddDbContext<AppointmentContext>(options => options.UseSqlite("Data Source=" + databaseLocation + appointmentContextConnectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
