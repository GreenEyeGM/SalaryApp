using Microsoft.EntityFrameworkCore;
using SalaryApp.Models;
using SalaryApp.Models.Enumerations;

namespace SalaryApp.Data;

public class SalaryAppDbContext : DbContext
{
    public SalaryAppDbContext()
    {
        
    }

    public SalaryAppDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ApplicationConfiguration.ConnectionString);
        }
    }
    
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Company> Companies { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Office> Offices { get; set; }
    public virtual DbSet<Position> Positions { get; set; }
    public virtual DbSet<Salary> Salaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure decimal precision
        modelBuilder.Entity<Position>()
            .Property(p => p.BaseSalary)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        modelBuilder.Entity<Salary>()
            .Property(s => s.Bonus)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);
        
        modelBuilder.Entity<Salary>()
            .Property(s => s.Deduction)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        // Configure relationships
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Address)
            .WithMany(a => a.Employees)
            .HasForeignKey(e => e.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Office)
            .WithMany(o => o.Employees)
            .HasForeignKey(e => e.OfficeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Salary>()
            .HasOne(s => s.Employee)
            .WithMany(e => e.Salaries)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Office>()
            .HasOne(o => o.Company)
            .WithMany(c => c.Offices)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Office>()
            .HasOne(o => o.Address)
            .WithMany(a => a.Offices)
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Position>()
            .HasOne(p => p.Department)
            .WithMany(d => d.Positions)
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Address>()
            .HasOne(a => a.City)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Set default values
        modelBuilder.Entity<Employee>()
            .Property(e => e.IsTerminated)
            .HasDefaultValue(false);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Cities
        var sofia = new City { Id = 1, Name = "Sofia" };
        var plovdiv = new City { Id = 2, Name = "Plovdiv" };
        var varna = new City { Id = 3, Name = "Varna" };

        modelBuilder.Entity<City>().HasData(sofia, plovdiv, varna);

        // Seed Companies
        var company1 = new Company { Id = 1, Name = "Tech Solutions Ltd." };
        var company2 = new Company { Id = 2, Name = "Digital Innovations Inc." };

        modelBuilder.Entity<Company>().HasData(company1, company2);

        // Seed Addresses
        var address1 = new Address 
        { 
            Id = 1, 
            StreetName = "Main Street", 
            StreetNumber = "1", 
            CityId = sofia.Id,
            AddressType = AddressType.Office
        };
        var address2 = new Address 
        { 
            Id = 2, 
            StreetName = "Tech Boulevard", 
            StreetNumber = "42", 
            CityId = plovdiv.Id,
            AddressType = AddressType.Office
        };

        modelBuilder.Entity<Address>().HasData(address1, address2);

        // Seed Offices
        var office1 = new Office { Id = 1, Name = "Sofia HQ", CompanyId = company1.Id, AddressId = address1.Id };
        var office2 = new Office { Id = 2, Name = "Plovdiv Branch", CompanyId = company2.Id, AddressId = address2.Id };

        modelBuilder.Entity<Office>().HasData(office1, office2);

        // Seed Departments
        var dept1 = new Department { Id = 1, Name = "IT", CompanyId = company1.Id };
        var dept2 = new Department { Id = 2, Name = "HR", CompanyId = company1.Id };
        var dept3 = new Department { Id = 3, Name = "Development", CompanyId = company2.Id };

        modelBuilder.Entity<Department>().HasData(dept1, dept2, dept3);

        // Seed Positions
        var pos1 = new Position { Id = 1, Title = "Senior Developer", DepartmentId = dept1.Id, BaseSalary = 5000m };
        var pos2 = new Position { Id = 2, Title = "HR Manager", DepartmentId = dept2.Id, BaseSalary = 4000m };
        var pos3 = new Position { Id = 3, Title = "Junior Developer", DepartmentId = dept3.Id, BaseSalary = 3000m };

        modelBuilder.Entity<Position>().HasData(pos1, pos2, pos3);

        // Seed Employees
        var employee1 = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            CompanyId = company1.Id,
            PositionId = pos1.Id,
            OfficeId = office1.Id,
            AddressId = address1.Id,
            HireDate = DateTime.Now.AddYears(-2),
            IsTerminated = false
        };

        var employee2 = new Employee
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            CompanyId = company1.Id,
            PositionId = pos2.Id,
            OfficeId = office1.Id,
            AddressId = address1.Id,
            HireDate = DateTime.Now.AddYears(-1),
            IsTerminated = false
        };

        var employee3 = new Employee
        {
            Id = 3,
            FirstName = "Bob",
            LastName = "Johnson",
            CompanyId = company2.Id,
            PositionId = pos3.Id,
            OfficeId = office2.Id,
            AddressId = address2.Id,
            HireDate = DateTime.Now.AddMonths(-6),
            IsTerminated = false
        };

        modelBuilder.Entity<Employee>().HasData(employee1, employee2, employee3);
    }
}