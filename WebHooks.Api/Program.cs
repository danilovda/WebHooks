using Microsoft.EntityFrameworkCore;
using WebHooks.Api.Data;
using WebHooks.Api.Extensions;
using WebHooks.Api.Models;
using WebHooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<WebhookDispatcher>();
builder.Services.AddDbContext<WebhooksDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("webhooks")));

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });

    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();

app.MapPost("webhook/subscription", async (
    CreateWebhookRequest request,
    WebhooksDbContext dbContext) =>
{
    WebhookSubscription subscription = new(
        Guid.NewGuid(),
        request.EventType,
        request.WebhookUrl,
        DateTime.UtcNow);
    
    dbContext.WebhookSubscriptions.Add(subscription);
    await dbContext.SaveChangesAsync();

    return Results.Ok(subscription);
});

app.MapPost("docs", async (
    CreateWebhookRequest request,
    WebhooksDbContext dbContext,
    WebhookDispatcher webhookDispatcher) =>
{
    var doc = new Doc 
    { 
        Id = Guid.NewGuid(),
        Timestamp = DateTime.UtcNow,
    };

    dbContext.Docs.Add(doc);

    await webhookDispatcher.DispatchAsync("doc.created", doc);

    return Results.Ok(doc);
})
    .WithTags("Docs"); ;

app.MapGet("/docs", async (WebhooksDbContext dbContext) =>
{
    return Results.Ok(await dbContext.Docs.ToListAsync());
})    
    .WithTags("Docs");

app.Run();
