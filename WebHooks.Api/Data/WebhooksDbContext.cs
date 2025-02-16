using Microsoft.EntityFrameworkCore;
using WebHooks.Api.Models;

namespace WebHooks.Api.Data;

internal sealed class WebhooksDbContext(DbContextOptions<WebhooksDbContext> options):DbContext(options)
{
    public DbSet<Doc> Docs { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doc>(builder =>
        {
            builder.ToTable("docs");
            builder.HasKey(d => d.Id);
        });

        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("subscriptions", "webhook");
            builder.HasKey(o => o.Id);
        });

        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("delivery_attempts", "webhook");
            builder.HasKey(o => o.Id);

            builder.HasOne<WebhookDeliveryAttempt>()
                .WithMany()
                .HasForeignKey(d => d.WebhookSubscriptionId);
        });
    }

}
//Add-Migration Create_Database