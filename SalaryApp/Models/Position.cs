using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryApp.Models;

public class Position
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Title { get; set; } = null!;
    
    public decimal BaseSalary { get; set; }
    
    [ForeignKey(nameof(Department))]
    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; } = null!;
    
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    
    
}