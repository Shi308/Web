using Microsoft.EntityFrameworkCore;
using Lab2.Data;
using Lab2.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Lab2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JSON options for Minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Minimal API endpoints for Movies
var moviesGroup = app.MapGroup("/api/movies");

moviesGroup.MapGet("/", async (Lab2Context db) =>
    await db.Movie.ToListAsync())
    .WithName("GetMovies")
    .Produces<List<Movie>>(StatusCodes.Status200OK);

moviesGroup.MapGet("/{id}", async (int id, Lab2Context db) =>
    await db.Movie.FindAsync(id)
        is Movie movie
            ? Results.Ok(movie)
            : Results.NotFound())
    .WithName("GetMovie")
    .Produces<Movie>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

moviesGroup.MapPost("/", async (MovieDTO movieDTO, Lab2Context db) =>
{
    var movie = new Movie
    {
        Title = movieDTO.Title,
        ReleaseDate = movieDTO.ReleaseDate,
        Genre = movieDTO.Genre,
        Price = movieDTO.Price,
        Rating = movieDTO.Rating
    };

    db.Movie.Add(movie);
    await db.SaveChangesAsync();

    return Results.Created($"/api/movies/{movie.Id}", movie);
})
.WithName("CreateMovie")
.Produces<Movie>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Accepts<MovieDTO>("application/json");

moviesGroup.MapPut("/{id}", async (int id, MovieDTO movieDTO, Lab2Context db) =>
{
    var movie = await db.Movie.FindAsync(id);

    if (movie is null) return Results.NotFound();

    movie.Title = movieDTO.Title;
    movie.ReleaseDate = movieDTO.ReleaseDate;
    movie.Genre = movieDTO.Genre;
    movie.Price = movieDTO.Price;
    movie.Rating = movieDTO.Rating;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("UpdateMovie")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status400BadRequest)
.Accepts<MovieDTO>("application/json");

moviesGroup.MapDelete("/{id}", async (int id, Lab2Context db) =>
{
    if (await db.Movie.FindAsync(id) is Movie movie)
    {
        db.Movie.Remove(movie);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
})
.WithName("DeleteMovie")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();
