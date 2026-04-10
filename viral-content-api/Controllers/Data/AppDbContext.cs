using Microsoft.EntityFrameworkCore;
using ViralContentApi.Models;

namespace ViralContentApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AiUsageRecord> AiUsageRecords { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<BillingEventLog> BillingEventLogs { get; set; }
        public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();
        public DbSet<WebhookEventLog> WebhookEventLogs => Set<WebhookEventLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WebhookEventLog>(entity =>
            {
                entity.Property(x => x.Provider).HasMaxLength(50).IsRequired();
                entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ExternalEventId).HasMaxLength(200);
                entity.Property(x => x.Message).HasMaxLength(1000);
                entity.Property(x => x.CustomerId).HasMaxLength(200);
                entity.Property(x => x.SubscriptionId).HasMaxLength(200);
                entity.Property(x => x.CheckoutSessionId).HasMaxLength(200);
                entity.Property(x => x.SubscriptionStatus).HasMaxLength(100);
                entity.Property(x => x.PlanName).HasMaxLength(100);
                entity.Property(x => x.BillingCycle).HasMaxLength(50);
            });

            modelBuilder.Entity<ProcessedWebhookEvent>(entity =>
            {
                entity.HasIndex(x => new { x.Provider, x.ExternalEventId }).IsUnique();
                entity.Property(x => x.Provider).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ExternalEventId).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Post>()
                .HasOne<User>()
                .WithMany(u => u.Posts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AiUsageRecord>()
                .HasOne(x => x.User)
                .WithMany(u => u.AiUsageRecords)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AiUsageRecord>()
                .HasIndex(x => new { x.UserId, x.UsageDateUtc })
                .IsUnique();

            modelBuilder.Entity<UserSubscription>()
                .HasOne(x => x.User)
                .WithMany(u => u.UserSubscriptions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSubscription>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<BillingEventLog>()
                .HasOne(x => x.User)
                .WithMany(u => u.BillingEventLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BillingEventLog>()
                .HasIndex(x => x.CreatedAtUtc);
        }
    }
}