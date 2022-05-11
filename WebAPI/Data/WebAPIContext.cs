#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class WebAPIContext : DbContext
    {
        public WebAPIContext (DbContextOptions<WebAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Tag> Tag { get; set; }

        public DbSet<Event> Event { get; set; }

        public DbSet<HostedEvent> HostedEvent { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            optionsBuilder
                .UseSqlServer(Configuration.GetConnectionString("WebAPIContext"))
                .EnableSensitiveDataLogging()
                .LogTo(x => System.Diagnostics.Debug.WriteLine(x));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Build Rooms
            modelBuilder.Entity<Room>()
                .HasData(
                    new Room
                    {
                        Id = 1,
                        Name = "F2-08",
                        Capacity = 150
                    },
                    new Room
                    {
                        Id = 2,
                        Name = "M1-11",
                        Capacity = 50
                    },
                    new Room
                    {
                        Id = 3,
                        Name = "M1-12",
                        Capacity = 76
                    },
                    new Room
                    {
                        Id = 4,
                        Name = "M1-14",
                        Capacity = 76
                    },
                    new Room
                    {
                        Id = 5,
                        Name = "M1-16",
                        Capacity = 50
                    },
                    new Room
                    {
                        Id = 6,
                        Name = "M2-11",
                        Capacity = 50
                    }
                );

            // Build Events
            modelBuilder.Entity<Event>()
                .HasData(
                    new Event
                    {
                        Id = 1,
                        Title = "Open Mic Comedy Night",
                        Description = "Come and participate in a night of laughs, or just come along for the free pizza and drinks!",
                    },
                    new Event
                    {
                        Id = 2,
                        Title = "Comedy Movie Marathon",
                        Description = "We're running back to back to back comedy movies all night long! Free popcorn and drinks."
                    },
                    new Event
                    {
                        Id = 3,
                        Title = "Horror Movie Marathon",
                        Description = "Up for a scare? How about hours worth of spine chilling entertainment? Then come on over, the slushies are freel, but bring your own snacks!"
                    }
                );

            // Give tags unique content
            //modelBuilder.Entity<Tag>()
                //.HasIndex(x => x.Content)
                //.IsUnique();

            // Build default tags
            modelBuilder.Entity<Tag>()
                .HasData(
                    new Tag { Id = 1, Content = "Comedy" },
                    new Tag { Id = 2, Content = "Movie" },
                    new Tag { Id = 3, Content = "Live" },
                    new Tag { Id = 4, Content = "Horror" }
                );

            // Build Many Events to Many Tags
            /// Causes an error on Update-Database
            modelBuilder.Entity<Event>()
                .HasMany(x => x.Tags)
                    .WithMany(e => e.Events)
                    .UsingEntity(j => j
                        .ToTable("EventTag")
                        .HasData(new[]
                        {
                            // Open mic tags
                            new { EventsId = 1, TagsId = 1 }, // Comedy
                            new { EventsId = 1, TagsId = 3 }, // Live
                            // Comedy movie tags
                            new { EventsId = 2, TagsId = 1 }, // Comedy
                            new { EventsId = 2, TagsId = 2 }, // Movie
                            // Horror movie tags
                            new { EventsId = 3, TagsId = 4 }, // Horror
                            new { EventsId = 3, TagsId = 2 }, // Movie
                        }
                  ));

            // Build HostedEvents
            modelBuilder.Entity<HostedEvent>()
                .HasData(
                    // Open Mic Comedy at 18:30 on 12/9/2022 in M1-11. Lasts 3 hours over 1 day.
                    // Costs £2 to enter.
                    new HostedEvent
                    {
                        Id = 1,
                        RoomId = 2,
                        _EventId = 1,
                        StartTime = new DateTime(2022, 9, 12, 18, 30, 0),
                        DurationMinutes = 0,
                        DurationHours = 3,
                        DurationDays = 1,
                        EntranceFee = 2f
                    },
                    // Comedy movie marathon at 16:00 on 12/9/2022 in F2-08. Lasts 4 hours over 2 days.
                    // Costs £1.50 to enter.
                    new HostedEvent
                    {
                        Id = 2,
                        RoomId = 1,
                        _EventId = 2,
                        StartTime = new DateTime(2022, 9, 12, 16, 0, 0),
                        DurationMinutes = 0,
                        DurationHours = 4,
                        DurationDays = 2,
                        EntranceFee = 1.5f
                    },
                    // Horror movie marathon at 21:45 on 12/9/2022 in M1-11. Lasts 4 and a half hours
                    // over 1 day. Free entry.
                    new HostedEvent
                    {
                        Id = 3,
                        RoomId = 2,
                        _EventId = 3,
                        StartTime = new DateTime(2022, 9, 12, 21, 45, 0),
                        DurationMinutes = 30,
                        DurationHours = 4,
                        DurationDays = 1,
                        EntranceFee = 0
                    }
                );
        }

        public DbSet<WebAPI.Models.Room> Room { get; set; }
    }
}
