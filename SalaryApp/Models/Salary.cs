using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryApp.Models;

public class Salary
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Employee))]
    public int EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime PeriodStart { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime PeriodEnd { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Deduction { get; set; }
}