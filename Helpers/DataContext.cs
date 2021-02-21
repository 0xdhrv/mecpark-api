using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sql server database
            options.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AllocationManager> AllocationManagers { get; set; }
        public DbSet<ParkingManager> ParkingManagers { get; set; }
        public DbSet<Garage> Garages { get; set; }
        public DbSet<Space> Spaces { get; set; }
        public DbSet<Parking> Parkings { get; set; }
        public DbSet<ParkingHistory> ParkingHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Space>()
                .HasOne(s => s.Garage)
                .WithMany(g => g.Spaces);
            modelBuilder.Entity<Space>()
                .HasOne(s => s.AllocationManager)
                .WithMany(a => a.Spaces);
            modelBuilder.Entity<Garage>()
                .HasOne(g => g.ParkingManager);
            modelBuilder.Entity<Parking>()
                .HasOne(p => p.User);
        }

    }
}