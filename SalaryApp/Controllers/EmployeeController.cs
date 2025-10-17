using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalaryApp.Data;
using SalaryApp.Models;
using SalaryApp.Models.Enumerations;
using SalaryApp.Models.ViewModels;

namespace SalaryApp.Controllers;

public class EmployeeController : Controller
{
    private readonly SalaryAppDbContext _context;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(SalaryAppDbContext context, ILogger<EmployeeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Employee
    public async Task<IActionResult> Index()
    {
        var employees = await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Position)
            .Include(e => e.Office)
            .Include(e => e.Address)
            .ThenInclude(a => a.City)
            .Where(e => !e.IsTerminated)
            .ToListAsync();
            
        return View(employees);
    }

    // GET: Employee/Create
    public async Task<IActionResult> Create()
    {
        // Load data for dropdowns
        ViewBag.Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
        ViewBag.Positions = new SelectList(await _context.Positions.ToListAsync(), "Id", "Title");
        ViewBag.Offices = new SelectList(await _context.Offices.ToListAsync(), "Id", "Name");
        ViewBag.Cities = new SelectList(await _context.Cities.ToListAsync(), "Id", "Name");
        
        return View(new EmployeeCreateViewModel { HireDate = DateTime.Now });
    }

    // POST: Employee/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel model)
    {
        // Re-populate ViewBag data in case of validation errors
        ViewBag.Companies = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name", model.CompanyId);
        ViewBag.Positions = new SelectList(await _context.Positions.ToListAsync(), "Id", "Title", model.PositionId);
        ViewBag.Offices = new SelectList(await _context.Offices.ToListAsync(), "Id", "Name", model.OfficeId);
        ViewBag.Cities = new SelectList(await _context.Cities.ToListAsync(), "Id", "Name", model.CityId);

        try
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Retrieve related entities
                    var company = await _context.Companies.FindAsync(model.CompanyId);
                    var position = await _context.Positions.FindAsync(model.PositionId);
                    var office = await _context.Offices.FindAsync(model.OfficeId);
                    var city = await _context.Cities.FindAsync(model.CityId);

                    if (company == null || position == null || office == null || city == null)
                    {
                        ModelState.AddModelError("", "One or more selected related entities (Company, Position, Office, City) could not be found.");
                        await transaction.RollbackAsync();
                        return View(model);
                    }

                    // Create new address
                    var address = new Address
                    {
                        StreetName = model.StreetName,
                        StreetNumber = model.StreetNumber,
                        Neighborhood = model.Neighborhood,
                        PostalCode = model.PostalCode,
                        CityId = model.CityId,
                        AddressType = AddressType.Employee,
                        City = city // Assign navigation property
                    };

                    _context.Addresses.Add(address);
                    await _context.SaveChangesAsync();

                    // Create new employee
                    var employee = new Employee
                    {
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName,
                        LastName = model.LastName,
                        HireDate = model.HireDate,
                        IsTerminated = false,
                        AddressId = address.Id,
                        PositionId = model.PositionId,
                        OfficeId = model.OfficeId,
                        CompanyId = model.CompanyId,
                        Address = address, // Assign navigation property
                        Company = company, // Assign navigation property
                        Office = office, // Assign navigation property
                        Position = position // Assign navigation property
                    };
                    
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during employee creation transaction.");
                    ModelState.AddModelError("", "An error occurred while saving the employee: " + ex.Message);
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError("Model State Error - Field: {FieldName}, Error: {ErrorMessage}", state.Key, error.ErrorMessage);
                    }
                }
                ModelState.AddModelError("", "Please correct the errors below.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during employee creation.");
            ModelState.AddModelError("", "An unexpected error occurred while saving the employee. Please try again.");
        }
        
        return View(model);
    }

    // GET: Employee/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Position)
            .Include(e => e.Office)
            .Include(e => e.Address)
            .ThenInclude(a => a.City)
            .Include(e => e.Salaries)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // GET: Employee/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Address)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (employee == null)
        {
            return NotFound();
        }

        var viewModel = new EmployeeEditViewModel
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            MiddleName = employee.MiddleName,
            LastName = employee.LastName,
            CompanyId = employee.CompanyId,
            PositionId = employee.PositionId,
            OfficeId = employee.OfficeId,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            IsTerminated = employee.IsTerminated,
            StreetName = employee.Address.StreetName,
            StreetNumber = employee.Address.StreetNumber,
            Neighborhood = employee.Address.Neighborhood,
            PostalCode = employee.Address.PostalCode,
            CityId = employee.Address.CityId,
            AddressType = employee.Address.AddressType
        };

        ViewBag.Companies = await _context.Companies.ToListAsync();
        ViewBag.Positions = await _context.Positions.ToListAsync();
        ViewBag.Offices = await _context.Offices.ToListAsync();
        ViewBag.Cities = await _context.Cities.ToListAsync();
        
        return View(viewModel);
    }

    // POST: Employee/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        // Re-populate ViewBag data in case of validation errors
        ViewBag.Companies = await _context.Companies.ToListAsync();
        ViewBag.Positions = await _context.Positions.ToListAsync();
        ViewBag.Offices = await _context.Offices.ToListAsync();
        ViewBag.Cities = await _context.Cities.ToListAsync();

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning($"Model State Error - Field: {error.ErrorMessage}");
            }
            return View(viewModel);
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Address)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                {
                    return NotFound();
                }

                // Update employee properties
                employee.FirstName = viewModel.FirstName;
                employee.MiddleName = viewModel.MiddleName;
                employee.LastName = viewModel.LastName;
                employee.CompanyId = viewModel.CompanyId;
                employee.PositionId = viewModel.PositionId;
                employee.OfficeId = viewModel.OfficeId;
                employee.HireDate = viewModel.HireDate;
                employee.TerminationDate = viewModel.TerminationDate;
                employee.IsTerminated = viewModel.IsTerminated;

                // Update address properties
                employee.Address.StreetName = viewModel.StreetName;
                employee.Address.StreetNumber = viewModel.StreetNumber;
                employee.Address.Neighborhood = viewModel.Neighborhood;
                employee.Address.PostalCode = viewModel.PostalCode;
                employee.Address.CityId = viewModel.CityId;
                employee.Address.AddressType = viewModel.AddressType;

                _context.Update(employee);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating employee");
                ModelState.AddModelError("", "An error occurred while updating the employee. Please try again.");
                return View(viewModel);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(viewModel.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }

    // GET: Employee/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Position)
            .Include(e => e.Office)
            .Include(e => e.Address)
            .ThenInclude(a => a.City)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // POST: Employee/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            // Mark as terminated instead of deleting
            employee.IsTerminated = true;
            employee.TerminationDate = DateTime.Now;
            _context.Update(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return NotFound();
    }

    // GET: Employee/Rehire/5
    public async Task<IActionResult> Rehire(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        if (!employee.IsTerminated)
        {
            return BadRequest("Employee is not terminated and cannot be rehired.");
        }

        return View(employee);
    }

    // POST: Employee/Rehire/5
    [HttpPost, ActionName("Rehire")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RehireConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        if (!employee.IsTerminated)
        {
            return BadRequest("Employee is not terminated.");
        }

        employee.IsTerminated = false;
        employee.TerminationDate = null; // Clear termination date upon rehire - nedovursheno
        employee.HireDate = DateTime.Now; // Set a new hire date upon rehire - nedovursheno

        _context.Update(employee);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}