using System.ComponentModel.DataAnnotations;
using SalaryApp.Models.Enumerations;

namespace SalaryApp.Models.ViewModels;

public class EmployeeCreateViewModel
{
    [Required]
    [MaxLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;
    
    [MaxLength(50)]
    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Required]
    [Display(Name = "Company")]
    public int CompanyId { get; set; }
    
    [Required]
    [Display(Name = "Position")]
    public int PositionId { get; set; }
    
    [Required]
    [Display(Name = "Office")]
    public int OfficeId { get; set; }
    
    [Required]
    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    // Address Information
    [Required]
    [MaxLength(100)]
    [Display(Name = "Street Name")]
    public string StreetName { get; set; } = null!;
    
    [Required]
    [MaxLength(15)]
    [Display(Name = "Street Number")]
    public string StreetNumber { get; set; } = null!;
    
    [MaxLength(30)]
    [Display(Name = "Neighborhood")]
    public string? Neighborhood { get; set; }
    
    [MaxLength(20)]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }
    
    [Required]
    [Display(Name = "City")]
    public int CityId { get; set; }

    [Required]
    [Display(Name = "Address Type")]
    public AddressType AddressType { get; set; }
} 