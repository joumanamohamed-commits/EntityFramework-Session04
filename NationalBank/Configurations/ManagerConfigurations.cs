using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class ManagerConfigurations : IEntityTypeConfiguration<Manager>
    {
        public void Configure(EntityTypeBuilder<Manager> builder)
        {
            builder.HasKey(M => M.Id);

            builder.Property(M => M.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(M => M.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(M => M.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(M => M.HireDate)
                .HasColumnType("date");

            builder.Property(M => M.BranchCode)
                .IsRequired()
                .HasMaxLength(10);

            #region One-to-One (Branch 1 ---- 1 Manager)
           
            builder.HasOne(M => M.Branch)
                .WithOne(B => B.Manager)
                .HasForeignKey<Manager>(M => M.BranchCode)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            SeedData(builder);
        }

        #region Data Seeding
        private static void SeedData(EntityTypeBuilder<Manager> builder)
        {
            builder.HasData(
                new Manager { Id = 1, FullName = "Khaled Mostafa", Email = "khaled.mostafa@nbank.com", PhoneNumber = "01001234567", HireDate = new DateTime(2015, 3, 1), BranchCode = "CAI-01" },
                new Manager { Id = 2, FullName = "Dina Samir", Email = "dina.samir@nbank.com", PhoneNumber = "01112345678", HireDate = new DateTime(2018, 7, 15), BranchCode = "ALX-01" },
                new Manager { Id = 3, FullName = "Hany Fawzy", Email = "hany.fawzy@nbank.com", PhoneNumber = "01223456789", HireDate = new DateTime(2020, 1, 10), BranchCode = "GIZ-01" }
            );
        }
        #endregion
    }
}
