namespace SalaryApp.Models.ViewModels;

public class SalaryCreateViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deduction { get; set; }
} 