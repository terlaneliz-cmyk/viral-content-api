using Microsoft.EntityFrameworkCore;
using ViralContentApi.Models;

namespace ViralContentApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<AiUsageRecord> AiUsageRecords => Set<AiUsageRecord>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<WebhookEventLog> WebhookEventLogs => Set<WebhookEventLog>();
    public DbSet<BillingEventLog> BillingEventLogs => Set<BillingEventLog>();
    public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();
    public DbSet<GeneratedContent> GeneratedContents => Set<GeneratedContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WebhookEventLog>()
            .HasIndex(x => x.EventId)
            .IsUnique();

        modelBuilder.Entity<BillingEventLog>()
            .HasIndex(x => x.UserId);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.ReferralCode)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => x.ReferredByUserId);

        modelBuilder.Entity<User>()
            .Property(x => x.Plan)
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(x => x.Role)
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(x => x.ReferralCode)
            .HasMaxLength(50);
    }
}