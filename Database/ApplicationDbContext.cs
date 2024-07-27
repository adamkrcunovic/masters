using FlightSearch.Database.Models;
using FlightSearch.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlightSearch.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Country> Countries { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<UserFriendRequest> UserFriendRequests { get; set; }

        public ApplicationDbContext(DbContextOptions options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserFriendRequest>()
                .HasOne(friendRequest => friendRequest.User1)
                .WithMany(user => user.SentFriendRequests)
                .HasForeignKey(friendRequest => friendRequest.UserId1);

            builder.Entity<UserFriendRequest>()
                .HasOne(friendRequest => friendRequest.User2)
                .WithMany(user => user.ReceivedFriendRequests)
                .HasForeignKey(friendRequest => friendRequest.UserId2);

            /* List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole{
                    Name = "User",
                    NormalizedName = "USER"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles); */
        }
    }
}