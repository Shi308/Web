using Microsoft.EntityFrameworkCore;
using Lab2.Models;

namespace Lab2.Data
{
    public class Lab2Context : DbContext
    {
        public Lab2Context(DbContextOptions<Lab2Context> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movie { get; set; } = default!;
    }
}





