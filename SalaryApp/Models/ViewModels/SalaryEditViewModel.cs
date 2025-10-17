using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SalaryApp.Models.ViewModels;

public class SalaryEditViewModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeFirstName { get; set; } = string.Empty;
    public string EmployeeLastName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime PeriodStart { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime PeriodEnd { get; set; }

    [Range(0, double.MaxValue)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public decimal Bonus { get; set; }

    [Range(0, double.MaxValue)]
    [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
    public decimal Deduction { get; set; }
} 