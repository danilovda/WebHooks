using WebHooks.Api.Models;
using WebHooks.Api.Repositories;

namespace WebHooks.Api.Services;

internal sealed class WebhookDispatcher(
    HttpClient httpClient,
    InMemoryWebhookSubscriptionRepository subscriptionRepository)
{
    public async Task DispatchAsync(string eventType, object payload)
    { 
        var subscriptions = subscriptionRepository.GetByEventType(eventType);

        foreach (WebhookSubscription webhookSubscription in subscriptions) 
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                webhookSubscription.EventType,
                SubscriptionId = webhookSubscription.Id,
                Timestamp = DateTime.UtcNow,
                Data = payload
            };

            await httpClient.PostAsJsonAsync(webhookSubscription.WebhookUrl, request);
        }
    }
}
