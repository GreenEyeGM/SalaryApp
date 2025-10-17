using System.ComponentModel.DataAnnotations;
using SalaryApp.Models.Enumerations;

namespace SalaryApp.Models.ViewModels;

public class EmployeeEditViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

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

    [Display(Name = "Termination Date")]
    [DataType(DataType.Date)]
    public DateTime? TerminationDate { get; set; }

    [Display(Name = "Is Terminated")]
    public bool IsTerminated { get; set; }

    // Address fields
    [Required]
    [Display(Name = "Street Name")]
    public string StreetName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Street Number")]
    public string StreetNumber { get; set; } = string.Empty;

    [Display(Name = "Neighborhood")]
    public string? Neighborhood { get; set; }

    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    [Required]
    [Display(Name = "City")]
    public int CityId { get; set; }

    [Required]
    [Display(Name = "Address Type")]
    public AddressType AddressType { get; set; }
} 