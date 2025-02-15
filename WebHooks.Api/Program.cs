using WebHooks.Api.Models;
using WebHooks.Api.Repositories;
using WebHooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryWebhookSubscriptionRepository>();

builder.Services.AddHttpClient<WebhookDispatcher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("webhook/subscription", (
    CreateWebhookRequest request,
    InMemoryWebhookSubscriptionRepository subscriptionRepository) =>
{
    WebhookSubscription subscription = new(Guid.NewGuid(), request.EventType, request.WebhookUrl, DateTime.UtcNow);
    
    subscriptionRepository.Add(subscription);
});

app.MapPost("docs", async (WebhookDispatcher webhookDispatcher) =>
{
    var doc = new { };
    await webhookDispatcher.DispatchAsync("doc.created", doc);
});

app.Run();
