using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebHooks.Api.Data;
using WebHooks.Api.Models;

namespace WebHooks.Api.Services;

internal sealed class WebhookDispatcher(
    IHttpClientFactory httpClientFactory,
    WebhooksDbContext dbContext)
{
    public async Task DispatchAsync<T>(string eventType, T data)
    {
        var subscriptions = await dbContext.WebhookSubscriptions
            .AsNoTracking()
            .Where(s => s.EventType == eventType)
            .ToListAsync();

        foreach (WebhookSubscription webhookSubscription in subscriptions)
        {
            using var httpClient = httpClientFactory.CreateClient();
            var payload = new WebhookPayload<T>()
            {
                Id = Guid.NewGuid(),
                EventType = webhookSubscription.EventType,
                SubscriptionId = webhookSubscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = data
            };
            var jsonPayload = JsonSerializer.Serialize(payload);

            try
            {
                var response = await httpClient.PostAsJsonAsync(webhookSubscription.WebhookUrl, payload);

                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = webhookSubscription.Id,
                    Payload = jsonPayload,
                    ResponseStatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    Timestamp = DateTime.UtcNow
                };

                dbContext.WebhookDeliveryAttempts.Add(attempt);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var attempt = new WebhookDeliveryAttempt
                {
                    Id = Guid.NewGuid(),
                    WebhookSubscriptionId = webhookSubscription.Id,
                    Payload = jsonPayload,
                    ResponseStatusCode = null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                };

                dbContext.WebhookDeliveryAttempts.Add(attempt);

                await dbContext.SaveChangesAsync();
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
