using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalaryApp.Data;
using SalaryApp.Models;
using SalaryApp.Models.ViewModels;
using System.Text;
using System.Globalization;

namespace SalaryApp.Controllers;

public class SalaryController : Controller
{
    private readonly SalaryAppDbContext _context;
    private readonly ILogger<SalaryController> _logger;

    public SalaryController(SalaryAppDbContext context, ILogger<SalaryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Salary
    public async Task<IActionResult> Index()
    {
        var salaries = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Position)
            .ToListAsync();
        return View(salaries);
    }

    // GET: Salary/Create/5
    public async Task<IActionResult> Create(int? employeeId)
    {
        if (employeeId == null)
        {
            return NotFound("Employee ID is required to create a salary record.");
        }

        var employee = await _context.Employees
            .Include(e => e.Position) 
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee == null)
        {
            return NotFound($"Employee with ID {employeeId} not found.");
        }

        // Safely access Position properties, providing defaults if Position is null
        string positionTitle = employee.Position?.Title ?? "N/A";
        decimal baseSalary = employee.Position?.BaseSalary ?? 0m;

        var viewModel = new SalaryCreateViewModel
        {
            EmployeeId = employee.Id,
            EmployeeFirstName = employee.FirstName,
            EmployeeLastName = employee.LastName,
            PositionTitle = positionTitle,
            BaseSalary = baseSalary,
            // Set default dates to current month
            PeriodStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
            PeriodEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
        };
        
        return View(viewModel);
    }

    // POST: Salary/Create/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("EmployeeId,PeriodStart,PeriodEnd,Bonus,Deduction")] SalaryCreateViewModel viewModel)
    {
        // Re-fetch employee and base salary for ViewModel in case of validation errors
        var employee = await _context.Employees
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == viewModel.EmployeeId);

        if (employee == null)
        {
            return NotFound();
        }

        // Assign back to ViewModel for consistent display on error, with null checks for Position
        viewModel.EmployeeFirstName = employee.FirstName;
        viewModel.EmployeeLastName = employee.LastName;
        viewModel.PositionTitle = employee.Position?.Title ?? "N/A";
        viewModel.BaseSalary = employee.Position?.BaseSalary ?? 0m;

        try
        {
            if (ModelState.IsValid)
            {
                // Validate period dates
                if (viewModel.PeriodStart > viewModel.PeriodEnd)
                {
                    ModelState.AddModelError("PeriodStart", "Period start date cannot be after period end date.");
                    return View(viewModel);
                }

                // Validate future dates
                if (viewModel.PeriodStart > DateTime.Now || viewModel.PeriodEnd > DateTime.Now)
                {
                    ModelState.AddModelError("PeriodStart", "Cannot create salary records for future dates.");
                    return View(viewModel);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Check if salary record already exists for this period
                    var existingSalary = await _context.Salaries
                        .Where(s => s.EmployeeId == viewModel.EmployeeId &&
                                   ((s.PeriodStart <= viewModel.PeriodStart && s.PeriodEnd >= viewModel.PeriodStart) ||
                                    (s.PeriodStart <= viewModel.PeriodEnd && s.PeriodEnd >= viewModel.PeriodEnd)))
                        .FirstOrDefaultAsync();

                    if (existingSalary != null)
                    {
                        ModelState.AddModelError("", "A salary record already exists for this period.");
                        return View(viewModel);
                    }

                    if (employee.IsTerminated)
                    {
                        ModelState.AddModelError("", "Cannot create salary records for terminated employees.");
                        return View(viewModel);
                    }

                    var salary = new Salary
                    {
                        EmployeeId = viewModel.EmployeeId,
                        PeriodStart = viewModel.PeriodStart,
                        PeriodEnd = viewModel.PeriodEnd,
                        Bonus = viewModel.Bonus,
                        Deduction = viewModel.Deduction
                    };

                    _context.Add(salary);
                    
                    try 
                    {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return RedirectToAction("Details", "Employee", new { id = viewModel.EmployeeId });
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                        return View(viewModel);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Error saving salary record: {ex.Message}");
                    return View(viewModel);
                }
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
        }

        return View(viewModel);
    }

    // GET: Salary/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salary = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Position)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (salary == null)
        {
            return NotFound();
        }

        var viewModel = new SalaryEditViewModel
        {
            Id = salary.Id,
            EmployeeId = salary.EmployeeId,
            EmployeeFirstName = salary.Employee.FirstName,
            EmployeeLastName = salary.Employee.LastName,
            PositionTitle = salary.Employee.Position.Title,
            BaseSalary = salary.Employee.Position.BaseSalary,
            PeriodStart = salary.PeriodStart,
            PeriodEnd = salary.PeriodEnd,
            Bonus = salary.Bonus,
            Deduction = salary.Deduction
        };

        return View(viewModel);
    }

    // POST: Salary/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,PeriodStart,PeriodEnd")] SalaryEditViewModel viewModel, string bonus, string deduction)
    {
        _logger.LogInformation($"Edit POST called for salary ID: {id}");
        
        if (id != viewModel.Id)
        {
            _logger.LogWarning($"ID mismatch: {id} != {viewModel.Id}");
            return NotFound();
        }

        // Get the existing salary record
        var existingSalary = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Position)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSalary == null)
        {
            return NotFound();
        }

        // Update the view model with employee information for display
        viewModel.EmployeeFirstName = existingSalary.Employee.FirstName;
        viewModel.EmployeeLastName = existingSalary.Employee.LastName;
        viewModel.PositionTitle = existingSalary.Employee.Position.Title;
        viewModel.BaseSalary = existingSalary.Employee.Position.BaseSalary;

        // Parse decimal values
        if (decimal.TryParse(bonus, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal bonusValue))
        {
            viewModel.Bonus = bonusValue;
        }
        else
        {
            ModelState.AddModelError("Bonus", "Invalid bonus value");
        }

        if (decimal.TryParse(deduction, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal deductionValue))
        {
            viewModel.Deduction = deductionValue;
        }
        else
        {
            ModelState.AddModelError("Deduction", "Invalid deduction value");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state is invalid");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning($"Validation error: {error.ErrorMessage}");
            }
            return View(viewModel);
        }

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate period dates
                if (viewModel.PeriodStart > viewModel.PeriodEnd)
                {
                    ModelState.AddModelError("PeriodStart", "Period start date cannot be after period end date.");
                    throw new Exception("Invalid period dates");
                }

                // Check if salary record already exists for this period (excluding current record)
                var duplicateSalary = await _context.Salaries
                    .Where(s => s.EmployeeId == viewModel.EmployeeId &&
                               s.Id != viewModel.Id &&
                               ((s.PeriodStart <= viewModel.PeriodStart && s.PeriodEnd >= viewModel.PeriodStart) ||
                                (s.PeriodStart <= viewModel.PeriodEnd && s.PeriodEnd >= viewModel.PeriodEnd)))
                    .FirstOrDefaultAsync();

                if (duplicateSalary != null)
                {
                    ModelState.AddModelError("", "A salary record already exists for this period.");
                    throw new Exception("Duplicate salary record");
                }

                // Update the existing salary record
                existingSalary.PeriodStart = viewModel.PeriodStart;
                existingSalary.PeriodEnd = viewModel.PeriodEnd;
                existingSalary.Bonus = viewModel.Bonus;
                existingSalary.Deduction = viewModel.Deduction;

                _logger.LogInformation($"Updating salary record {existingSalary.Id} for employee {existingSalary.EmployeeId}");
                _context.Update(existingSalary);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Salary record updated successfully");
                return RedirectToAction("Details", "Employee", new { id = existingSalary.EmployeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating salary record");
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SalaryExists(existingSalary.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the salary record");
            ModelState.AddModelError("", "An error occurred while updating the salary record. Please try again.");
            return View(viewModel);
        }
    }

    // GET: Salary/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salary = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Position)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (salary == null)
        {
            return NotFound();
        }

        ViewBag.Employee = salary.Employee;
        ViewBag.BaseSalary = salary.Employee.Position.BaseSalary;

        return View(salary);
    }

    // POST: Salary/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary != null)
        {
            var employeeId = salary.EmployeeId;
            _context.Salaries.Remove(salary);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Employee", new { id = employeeId });
        }
        return NotFound();
    }

    // GET: Salary/ViewPdf/5
    public async Task<IActionResult> ViewPdf(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var salary = await _context.Salaries
            .Include(s => s.Employee)
            .ThenInclude(e => e.Position)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (salary == null)
        {
            return NotFound();
        }

        // Generate HTML content
        var htmlContent = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 40px; }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .details {{ margin-bottom: 20px; }}
                    .row {{ margin-bottom: 10px; }}
                    .label {{ font-weight: bold; }}
                    .total {{ margin-top: 20px; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Salary Statement</h1>
                </div>
                <div class='details'>
                    <div class='row'>
                        <span class='label'>Employee:</span>
                        <span>{salary.Employee.FirstName} {salary.Employee.LastName}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Position:</span>
                        <span>{salary.Employee.Position.Title}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Period:</span>
                        <span>{salary.PeriodStart.ToShortDateString()} - {salary.PeriodEnd.ToShortDateString()}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Base Salary:</span>
                        <span>{salary.Employee.Position.BaseSalary:C}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Bonus:</span>
                        <span>{salary.Bonus:C}</span>
                    </div>
                    <div class='row'>
                        <span class='label'>Deduction:</span>
                        <span>{salary.Deduction:C}</span>
                    </div>
                    <div class='row total'>
                        <span class='label'>Total:</span>
                        <span>{(salary.Employee.Position.BaseSalary + salary.Bonus - salary.Deduction):C}</span>
                    </div>
                </div>
            </body>
            </html>";

        // Convert HTML to PDF using browser's print functionality
        return new ContentResult
        {
            ContentType = "text/html",
            Content = htmlContent
        };
    }

    private bool SalaryExists(int id)
    {
        return _context.Salaries.Any(e => e.Id == id);
    }
} 