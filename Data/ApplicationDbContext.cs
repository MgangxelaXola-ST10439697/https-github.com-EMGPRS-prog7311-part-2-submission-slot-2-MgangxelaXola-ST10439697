using Global_Logistics_Managemant_System_POE.Models;
using Global_Logistics_Managemant_System_POE.Patterns.Factory;
using Microsoft.EntityFrameworkCore;

namespace Global_Logistics_Managemant_System_POE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Inheritance: Contract (base) + SLAContract (derived) 
            modelBuilder.Entity<Contract>()
                .HasDiscriminator<string>("ContractType")
                .HasValue<Contract>("Standard")
                .HasValue<SLAContract>("SLA");

            // SLA-specific columns 
            modelBuilder.Entity<SLAContract>()
                .Property(s => s.SlaTerms).HasColumnName("SlaTerms");
            modelBuilder.Entity<SLAContract>()
                .Property(s => s.ResponseTime).HasColumnName("ResponseTime");
            modelBuilder.Entity<SLAContract>()
                .Property(s => s.PenaltyClause).HasColumnName("PenaltyClause");

            

            // Client → Contracts 
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contract → ServiceRequests 
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Client>()
                .Ignore(c => c.ServiceRequests);
        }
    }
}