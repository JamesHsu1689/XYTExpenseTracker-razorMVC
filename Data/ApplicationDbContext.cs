using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Data;

//public class ApplicationDbContext : DbContext
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Constructor to pass options to the base DbContext
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }



    //Data models
    //public DbSet<Person> Persons { get; set; } //Old way of defining DbSet

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<FundTransaction> FundTransactions => Set<FundTransaction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        b.Entity<Event>(e =>
        {
            e.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
            e.Property(x => x.PerPersonAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.SurplusOrDeficit).HasColumnType("decimal(18,2)");
        });

        b.Entity<Payment>(p => p.Property(x => x.Amount).HasColumnType("decimal(18,2)"));
        b.Entity<FundTransaction>(f => f.Property(x => x.Amount).HasColumnType("decimal(18,2)"));
    }
}
