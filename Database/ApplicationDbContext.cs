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
        public DbSet<Itinerary> Itinenaries { get; set; }
        public DbSet<FlightSegment> FlightSegments { get; set; }
        public DbSet<ItineraryMember> ItineraryMembers { get; set; }
        public DbSet<Comment> Comments { get; set; }

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

            builder.Entity<ItineraryMember>()
                .HasOne(itineraryMember => itineraryMember.Itinerary)
                .WithMany(itinerary => itinerary.InvitedMembers)
                .HasForeignKey(itineraryMember => itineraryMember.ItineraryId);

            builder.Entity<ItineraryMember>()
                .HasOne(itineraryMember => itineraryMember.User)
                .WithMany(user => user.ReceivedItineraryRequests)
                .HasForeignKey(itineraryMember => itineraryMember.UserId);

            builder.Entity<User>()
                .Property(user => user.Name)
                .HasDefaultValue("Adam");

            builder.Entity<User>()
                .Property(user => user.LastName)
                .HasDefaultValue("Krcunovic");

            builder.Entity<User>()
                .Property(user => user.Birthday)
                .HasDefaultValue(DateOnly.Parse("1997-12-24"));

            builder.Entity<User>()
                .Property(user => user.CountryId)
                .HasDefaultValue(1);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole{
                    Name = "User",
                    NormalizedName = "USER"
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}