using Microsoft.EntityFrameworkCore;

namespace NDTField.Web.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Building> Buildings { get; set; }
}