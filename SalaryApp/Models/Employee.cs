using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryApp.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(50)]
    public string? MiddleName { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;

    [ForeignKey(nameof(Address))]
    public int AddressId { get; set; }
    public virtual Address Address { get; set; } = null!;

    [Required] 
    public DateTime HireDate { get; set; }
    
    public DateTime? TerminationDate { get; set; }
    
    public bool IsTerminated { get; set; }
    
    [ForeignKey(nameof(Position))]
    public int PositionId { get; set; }
    public virtual Position Position { get; set; } = null!;
    
    [ForeignKey(nameof(Office))]
    public int OfficeId { get; set; }
    public virtual Office Office { get; set; } = null!;
    
    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Salary> Salaries { get; set; } = new HashSet<Salary>();
}