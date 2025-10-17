using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SalaryApp.Models.Enumerations;

namespace SalaryApp.Models;

public class Address
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string StreetName { get; set; } = null!;
    
    [Required]
    [MaxLength(15)]
    public string StreetNumber { get; set; } = null!;
    
    [MaxLength(30)]
    public string? Neighborhood { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [ForeignKey(nameof(City))]
    public int CityId { get; set; }
    public virtual City City { get; set; } = null!;
    
    public AddressType AddressType { get; set; }
    
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
    
    public virtual ICollection<Office> Offices { get; set; } = new HashSet<Office>();
}