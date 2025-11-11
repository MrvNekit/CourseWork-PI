using AppointmentApplication.DatabaseContexts;
using AppointmentApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text.RegularExpressions;

namespace AppointmentApplication.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly AppointmentContext _appointmentContext;
        private readonly AccountContext _accountContext;
        private readonly IConfiguration _сonfiguration;

        private readonly WorkDayOptions _options;

        public AppointmentController(AppointmentContext appointmentContext, AccountContext accountContext, IConfiguration configuration)
        {
            _appointmentContext = appointmentContext;
            _accountContext = accountContext;
            _сonfiguration = configuration;

            _options = new WorkDayOptions();
            _сonfiguration.GetSection(WorkDayOptions.Position).Bind(_options);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new AppointmentDataModel();
            DateTime date = DateTime.Now;
            TimeSpan appointmentEnd = date.AddMinutes(_options.AppointmentDuration.TotalMinutes).TimeOfDay;
            if (appointmentEnd >= _options.EndTime ||
                appointmentEnd < _options.StartTime)
            {
                date = date.AddDays(1);
            }

            model.Date = date.ToString("yyyy-MM-dd");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AppointmentDataModel model)
        {
            if(!TryValidateModel(model))
            {
                ModelState.AddModelError("FirstName", "Ошибка при вводе данных!");
                return View(model);
            }

            Match match = Regex.Match(model.SelectedNumber, @"^(\d{2}:\d{2}) - (\d{2}:\d{2});(\d+)$");
            
            if (!match.Success)
            {
                ModelState.AddModelError(model.SelectedNumber, "Выбрано неправильное время!");
                return View(model);
            }
            
            int id = int.Parse(match.Groups[3].Value);

            Console.BackgroundColor = ConsoleColor.Green;
            // Use external services to confirm Passport and SNILS
            TimeSpan startTime = _options.StartTime;
            TimeSpan endTime = _options.EndTime;
            TimeSpan duration = _options.AppointmentDuration;
            TimeSpan selectedStartTime = TimeSpan.Parse(match.Groups[1].Value);
            TimeSpan selectedEndTime = TimeSpan.Parse(match.Groups[2].Value);

            TimeSpan availableTime = CalculateAvailableTime(selectedStartTime, startTime, duration);
            TimeSpan availableEndTime = availableTime + _options.AppointmentDuration;

            Console.WriteLine(startTime);
            Console.WriteLine(availableTime);
            Console.WriteLine(availableEndTime);
            Console.WriteLine(selectedStartTime);

            if (availableTime < startTime || availableEndTime > endTime || 
                availableTime != selectedStartTime || availableEndTime != selectedEndTime)
            {
                ModelState.AddModelError(model.SelectedNumber, "Выбрано неправильное время!");
                return View(model);
            }

            _appointmentContext.Appointments.Add(new AppointmentModel()
            {
                SpecialistId = id,
                Date = model.Date,
                StartTime = match.Groups[1].Value,
                EndTime = match.Groups[2].Value,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                SNILS = model.SNILS,
                PassportSeries = model.PassportSeries,
                PassportNumber = model.PassportNumber,
                PhoneNumber = model.PhoneNumber,
            
            });
            await _appointmentContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Вы успешно удалили запись!";

            return View(model);
        }

        [HttpGet]
        public IActionResult Discard()
        {
            AppointmentDataModel model = new AppointmentDataModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Discard(AppointmentDataModel model)
        {
            AppointmentModel? appointment = await _appointmentContext.Appointments.Where(x => x.FirstName == model.FirstName &&
                                                                                              x.MiddleName == model.MiddleName &&
                                                                                              x.LastName == model.LastName &&
                                                                                              x.SNILS == model.SNILS &&
                                                                                              x.PassportNumber == model.PassportNumber &&
                                                                                              x.PassportSeries == model.PassportSeries &&
                                                                                              x.PhoneNumber == model.PhoneNumber).FirstOrDefaultAsync();
            // Use Phone validation service
            
            if(appointment == null)
            {
                ModelState.AddModelError("SelectedNumber", "Не удалось удалить запись!");
                return View(model);
            }
            _appointmentContext.Appointments.Remove(appointment);
            await _appointmentContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Вы успешно создали запись!";

            return View(model);
        }

        [HttpPost]
        public async Task<PartialViewResult> GetSchedule([FromBody] ScheduleSelection selectedTime)
        {
            return PartialView("_SchedulePartialView", await GetAppointmentTimes(selectedTime));
        }

        public async Task<List<TimePartModel>> GetAppointmentTimes(ScheduleSelection selectedTime)
        {
            List<AppointmentModel> models = await _appointmentContext.Appointments.Where(x => x.Date == selectedTime.Date).ToListAsync();

            List<TimePartModel> appointmentTimes = new List<TimePartModel>();

            TimeSpan startTime = _options.StartTime;
            TimeSpan endTime = _options.EndTime;
            TimeSpan duration = _options.AppointmentDuration;

            DateTime selectedDate = DateTime.Parse(selectedTime.Date);
            DateTime now = DateTime.Now;
            DateTime startDateTime = now.Date + startTime;

            if (selectedDate < now.Date)
                return [];

            if (selectedDate == now.Date)
                startTime = CalculateAvailableTime(now.TimeOfDay, startTime, duration) + duration;

            for (TimeSpan time = startTime;  time < endTime; time += duration)
            {
                List<AccountModel> employees = await _accountContext.Accounts.Where(x => x.Speciality.Contains(selectedTime.Direction)).ToListAsync();

                if (employees.Count == 0) continue;

                foreach (AppointmentModel model in models)
                {
                    if (TimeSpan.Parse(model.StartTime) != time)
                        continue;
                    AccountModel? target = await _accountContext.Accounts.FindAsync(model.SpecialistId);
                    if(target != null)
                        employees.Remove(target);
                }

                if (employees.Count == 0) continue;

                TimePartModel appointmentTime = new TimePartModel();
                appointmentTime.Time = string.Format("{0} - {1}", time.ToString(@"hh\:mm"), (time + duration).ToString(@"hh\:mm"));
                appointmentTime.Users = new List<Fullname>();

                foreach (AccountModel model in employees)
                    appointmentTime.Users.Add(new Fullname() { FirstName = model.FirstName, MiddleName = model.MiddleName, LastName = model.LastName, Id = model.Id });

                appointmentTimes.Add(appointmentTime);
            }

            return appointmentTimes;
        }

        public TimeSpan CalculateAvailableTime(TimeSpan now, TimeSpan startTime, TimeSpan duration)
        {
            TimeSpan timeSpan = now;

            TimeSpan timeSinceStart = timeSpan - startTime;
            double completedDurations = Math.Floor(timeSinceStart.TotalMinutes / duration.TotalMinutes);
            Console.WriteLine(completedDurations);
            return startTime.Add(TimeSpan.FromMinutes(completedDurations * duration.TotalMinutes));
        }

        [HttpPost]
        public async Task<PartialViewResult> GetUserAppointments([FromBody] AppointmentDataModel model)
        {
            List<AppointmentModel> models = await _appointmentContext.Appointments.Where(x => x.FirstName == model.FirstName &&
                                                                                              x.MiddleName == model.MiddleName &&
                                                                                              x.LastName == model.LastName &&
                                                                                              x.SNILS == model.SNILS &&
                                                                                              x.PassportNumber == model.PassportNumber &&
                                                                                              x.PassportSeries == model.PassportSeries &&
                                                                                              x.PhoneNumber == model.PhoneNumber).ToListAsync();
            List<TimePartModel> scheduleSelection = new List<TimePartModel>();
            foreach (AppointmentModel appointment in models)
            {
                AccountModel? account = await _accountContext.Accounts.FindAsync(appointment.SpecialistId);
                if (account == null) continue;
            
                scheduleSelection.Add(new TimePartModel()
                {
                    Time = appointment.StartTime + " - " + appointment.EndTime,
                    Users = new List<Fullname>() { new Fullname() 
                    {
                        FirstName = account.FirstName,
                        MiddleName = account.MiddleName,
                        LastName = account.LastName,
                        Id = appointment.Id
                    } }
                });
            }

            return PartialView("_SchedulePartialView", scheduleSelection);
        }
    }
}
