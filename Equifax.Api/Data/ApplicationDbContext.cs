using Equifax.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Equifax.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        DbSet<DisputeRequest> DisputeRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DisputeRequest>()
                .Property(d => d.RequestStatus)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
