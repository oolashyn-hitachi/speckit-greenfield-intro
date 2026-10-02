using RSSFeedReader.Api.Models;
using RSSFeedReader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddSingleton<InMemorySubscriptionStore>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalBlazorClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5213", "https://localhost:7025")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseRouting();
app.UseCors("LocalBlazorClient");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/subscriptions", (InMemorySubscriptionStore store) =>
    Results.Ok(store.GetAll()));

app.MapPost("/api/subscriptions", (AddSubscriptionRequest? request, InMemorySubscriptionStore store) =>
{
    if (!store.TryAdd(request?.Url, out var added))
    {
        return (IResult)Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["url"] = ["A non-whitespace URL is required."]
        });
    }

    return Results.Ok(added);
});

app.Run();
