using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryApp.Models;

public class Office
{
    [Key] 
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;
    
    [ForeignKey(nameof(Address))]
    public int AddressId { get; set; }
    public virtual Address Address { get; set; } = null!;
    
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}