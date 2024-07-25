using FlightSearch.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlightSearch.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
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