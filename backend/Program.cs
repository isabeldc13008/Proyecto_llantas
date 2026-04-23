using TireControl.Api.Models;
using TireControl.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RecordRepository>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();

app.MapGet("/api/records", (RecordRepository repo, string? type) =>
{
    var data = repo.GetAll();
    if (!string.IsNullOrWhiteSpace(type))
    {
        data = data.Where(r => string.Equals(r.Type, type, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(data.OrderByDescending(r => r.CreatedAt));
});

app.MapPost("/api/records", (RecordRepository repo, Record record) =>
{
    var created = repo.Create(record);
    return Results.Created($"/api/records/{created.Id}", created);
});

app.MapPut("/api/records/{id:guid}", (RecordRepository repo, Guid id, Record record) =>
{
    var updated = repo.Update(id, record);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

app.MapDelete("/api/records/{id:guid}", (RecordRepository repo, Guid id) =>
{
    var deleted = repo.Delete(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.Run();
