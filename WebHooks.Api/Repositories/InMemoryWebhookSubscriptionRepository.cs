using WebHooks.Api.Models;

namespace WebHooks.Api.Repositories;

public class InMemoryWebhookSubscriptionRepository
{
    private readonly List<WebhookSubscription> _subscriptions = [];

    public void Add(WebhookSubscription webhookSubscription)
    { 
        _subscriptions.Add(webhookSubscription);
    }

    public IReadOnlyList<WebhookSubscription> GetByEventType(string eventType)
    { 
        return _subscriptions.Where(s => s.EventType == eventType).ToList().AsReadOnly();
    }

}
