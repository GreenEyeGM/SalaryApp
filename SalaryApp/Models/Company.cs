using System.ComponentModel.DataAnnotations;

namespace SalaryApp.Models;

public class Company
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    public virtual ICollection<Office> Offices { get; set; } = new HashSet<Office>();
    public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}