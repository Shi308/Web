using System.Collections.Concurrent;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var todos = new ConcurrentDictionary<int, TodoItem>();
var nextId = 1;

app.MapGet("/todos", () =>
{
    var results = todos.Values.Select(ToDto);
    return Results.Ok(results);
})
.WithName("GetTodos")
.WithOpenApi();

app.MapGet("/todos/{id:int}", (int id) =>
    todos.TryGetValue(id, out var item)
        ? Results.Ok(ToDto(item))
        : Results.NotFound())
.WithName("GetTodoById")
.WithOpenApi();

app.MapPost("/todos", (TodoItemCreateDto dto) =>
{
    var id = Interlocked.Increment(ref nextId);
    var item = new TodoItem
    {
        Id = id,
        Title = dto.Title,
        IsComplete = dto.IsComplete,
        DueDate = dto.DueDate
    };

    if (!todos.TryAdd(id, item))
    {
        return Results.Problem("Could not create todo.", statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.CreatedAtRoute("GetTodoById", new { id }, ToDto(item));
})
.WithName("CreateTodo")
.WithOpenApi();

app.MapPut("/todos/{id:int}", (int id, TodoItemUpdateDto dto) =>
{
    if (!todos.TryGetValue(id, out var existing))
    {
        return Results.NotFound();
    }

    var updated = existing with
    {
        Title = dto.Title ?? existing.Title,
        IsComplete = dto.IsComplete ?? existing.IsComplete,
        DueDate = dto.DueDate ?? existing.DueDate
    };

    todos[id] = updated;
    return Results.Ok(ToDto(updated));
})
.WithName("UpdateTodo")
.WithOpenApi();

app.MapDelete("/todos/{id:int}", (int id) =>
{
    return todos.TryRemove(id, out _)
        ? Results.NoContent()
        : Results.NotFound();
})
.WithName("DeleteTodo")
.WithOpenApi();

app.Run();

static TodoItemDto ToDto(TodoItem item) => new(item.Id, item.Title, item.IsComplete, item.DueDate);

internal record TodoItem
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsComplete { get; init; }
    public DateOnly? DueDate { get; init; }
}

internal record TodoItemDto(int Id, string Title, bool IsComplete, DateOnly? DueDate);

internal record TodoItemCreateDto
{
    public string Title { get; init; } = string.Empty;
    public bool IsComplete { get; init; }
    public DateOnly? DueDate { get; init; }
}

internal record TodoItemUpdateDto
{
    public string? Title { get; init; }
    public bool? IsComplete { get; init; }
    public DateOnly? DueDate { get; init; }
}
