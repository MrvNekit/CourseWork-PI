using AppointmentApplication.DatabaseContexts;
using AppointmentApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AppointmentApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountContext _accountContext;
        private readonly AppointmentContext _appointmentContext;
        private readonly IConfiguration _сonfiguration;

        private readonly WorkDayOptions _options;

        public AccountController(AccountContext accountContext, AppointmentContext appointmentContext, IConfiguration configuration)
        {
            _accountContext = accountContext;
            _appointmentContext = appointmentContext;
            _сonfiguration = configuration;

            _options = new WorkDayOptions();
            _сonfiguration.GetSection(WorkDayOptions.Position).Bind(_options);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AuthentificationModel model)
        {
            AccountModel? accountModel = await _accountContext.Accounts.FirstOrDefaultAsync(x => x.Login == model.Login);

            if (accountModel == null)
            {
                ModelState.AddModelError("Login", "Аккаунт не найден!");
                return View(model);
            }

            if(accountModel.Password != model.Password)
            {
                ModelState.AddModelError("Password", "Неверный пароль!");
                return View(model);
            }

            List<Claim> claims = new List<Claim> { 
                new Claim(ClaimTypes.Name, accountModel.Login),
                new Claim(ClaimTypes.Role, accountModel.Role)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<PartialViewResult> GetSchedule([FromBody] ScheduleSelection selectedTime)
        {
            return PartialView("_SchedulePartialView", await GetAppointmentsTimetable(selectedTime));
        }

        public async Task<PartialViewResult> GetAppointmentInfo([FromBody] string selectedId)
        {
            if(selectedId == null)
                return PartialView("_AppointmentInfoPartialView", null);

            Match match = Regex.Match(selectedId, @"^(\d{2}:\d{2}) - (\d{2}:\d{2});(\d+)$");

            if (!match.Success)
            {
                return PartialView("_AppointmentInfoPartialView", null);
            }

            int id = int.Parse(match.Groups[3].Value);

            return PartialView("_AppointmentInfoPartialView", await _appointmentContext.Appointments.FindAsync(id));
        }

        public async Task<List<TimePartModel>?> GetAppointmentsTimetable(ScheduleSelection selectedTime)
        {
            if (HttpContext.User.Identity == null) return null;
            
            string? login = HttpContext.User.Identity.Name;

            AccountModel account = await _accountContext.Accounts.FirstAsync(x => x.Login == login);

            List<AppointmentModel> models = await _appointmentContext.Appointments.Where(x => x.Date == selectedTime.Date && x.SpecialistId == account.Id).ToListAsync();

            List<TimePartModel> appointmentTimes = new List<TimePartModel>();

            TimeSpan startTime = _options.StartTime;
            TimeSpan endTime = _options.EndTime;
            TimeSpan duration = _options.AppointmentDuration;

            for (TimeSpan time = startTime; time <= endTime; time += duration)
            {
                TimePartModel appointmentTime = new TimePartModel();
                appointmentTime.Time = string.Format("{0} - {1}", time.ToString(@"hh\:mm"), (time + duration).ToString(@"hh\:mm"));
                appointmentTime.Users = new List<Fullname>();

                foreach (AppointmentModel model in models)
                {
                    if (model.StartTime != null && TimeSpan.Parse(model.StartTime) != time)
                        continue;
                    appointmentTime.Users.Add(new Fullname()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        MiddleName = model.MiddleName,
                        Id = model.Id,
                    });
                }
            
                appointmentTimes.Add(appointmentTime);
            }

            return appointmentTimes;
        }
    }
}
