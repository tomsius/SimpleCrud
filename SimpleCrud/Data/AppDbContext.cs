using Microsoft.EntityFrameworkCore;
using SimpleCrud.Models.Domain;

namespace SimpleCrud.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<User> Users { get; set; }
}
