using Equifax.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Equifax.Api.AppDbContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        DbSet<RequestMaster> RequestMaster { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestMaster>()
                .Property(d => d.request_status)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
