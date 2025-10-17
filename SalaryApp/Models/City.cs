using System.ComponentModel.DataAnnotations;

namespace SalaryApp.Models;

public class City
{
    [Key]
    public int Id { get; set; }

    [Required] [MaxLength(100)] 
    public string Name { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
}