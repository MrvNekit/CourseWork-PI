using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentApplication.DatabaseContexts;
using AppointmentApplication.Models;
using Microsoft.AspNetCore.Authorization;

namespace AppointmentApplication.Controllers.Editors
{
    [Authorize(Roles = "admin")]
    public class AccountItemsController : Controller
    {
        private readonly AccountContext _context;

        public AccountItemsController(AccountContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Accounts.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountModel = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountModel == null)
            {
                return NotFound();
            }

            return View(accountModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,MiddleName,LastName,Role,Speciality,Login,Password")] AccountModel accountModel)
        {
            if (ModelState.IsValid)
            {
                if (accountModel.Speciality != null && accountModel.Speciality.Count == 1)
                    accountModel.Speciality = accountModel.Speciality.First().Split(";").ToList();
                _context.Add(accountModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(accountModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountModel = await _context.Accounts.FindAsync(id);
            if (accountModel == null)
            {
                return NotFound();
            }
            return View(accountModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,Role,Speciality,Login,Password")] AccountModel accountModel)
        {
            if (id != accountModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(accountModel.Speciality != null && accountModel.Speciality.First() != null)
                        accountModel.Speciality = accountModel.Speciality.First().Split(";").ToList();
                    _context.Update(accountModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountModelExists(accountModel.Id))
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
            return View(accountModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var accountModel = await _context.Accounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (accountModel == null)
            {
                return NotFound();
            }

            return View(accountModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var accountModel = await _context.Accounts.FindAsync(id);
            if (accountModel != null)
            {
                _context.Accounts.Remove(accountModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountModelExists(int id)
        {
            return _context.Accounts.Any(e => e.Id == id);
        }
    }
}
