using AppointmentApplication.DatabaseContexts;
using AppointmentApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentApplication.Controllers.Editors
{
    [Authorize(Roles = "admin")]
    public class AppointmentItemsController : Controller
    {
        private readonly AppointmentContext _context;

        public AppointmentItemsController(AppointmentContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Appointments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointmentModel = await _context.Appointments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointmentModel == null)
            {
                return NotFound();
            }

            return View(appointmentModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SpecialistId,Date,StartTime,EndTime,FirstName,MiddleName,LastName,SNILS,PassportSeries,PassportNumber,PhoneNumber")] AppointmentModel appointmentModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointmentModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appointmentModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointmentModel = await _context.Appointments.FindAsync(id);
            if (appointmentModel == null)
            {
                return NotFound();
            }
            return View(appointmentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SpecialistId,Date,StartTime,EndTime,FirstName,MiddleName,LastName,SNILS,PassportSeries,PassportNumber,PhoneNumber")] AppointmentModel appointmentModel)
        {
            if (id != appointmentModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointmentModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentModelExists(appointmentModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(appointmentModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointmentModel = await _context.Appointments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointmentModel == null)
            {
                return NotFound();
            }

            return View(appointmentModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointmentModel = await _context.Appointments.FindAsync(id);
            if (appointmentModel != null)
            {
                _context.Appointments.Remove(appointmentModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentModelExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
