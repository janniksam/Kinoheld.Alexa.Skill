using Kinoheld.Domain.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Kinoheld.Domain.Services.Database
{
    public class KinoheldDbContext : DbContext
    {
        public KinoheldDbContext(DbContextOptions<KinoheldDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<KinoheldUser> User { get; set; }

        public DbSet<CityCinemaAssignment> CityCinemaAssignment { get; set; }
    }
}