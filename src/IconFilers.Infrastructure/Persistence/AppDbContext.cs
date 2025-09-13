using IconFilers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IconFilers.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<PaymentMode> PaymentModes { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ClientDocument> ClientDocuments { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Role> Roles { get; set; }
}
