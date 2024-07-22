using FlightSearch.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightSearch.Database
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }

        public ApplicationDbContext(DbContextOptions options): base(options)
        {

        }

        /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().HasKey(country => country.Id);
        } */
    }
}