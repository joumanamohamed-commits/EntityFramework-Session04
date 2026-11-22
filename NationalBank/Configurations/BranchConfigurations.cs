using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class BranchConfigurations : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.HasKey(B => B.Code);

            builder.Property(B => B.Code)
                .HasMaxLength(10);

            builder.Property(B => B.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(B => B.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(B => B.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            SeedData(builder);
        }

        #region Data Seeding
        private static void SeedData(EntityTypeBuilder<Branch> builder)
        {
            builder.HasData(
                new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "15 Tahrir Square, Cairo", PhoneNumber = "0223456789" },
                new Branch { Code = "ALX-01", Name = "Alexandria Branch", Address = "40 Fouad Street, Alexandria", PhoneNumber = "034876543" },
                new Branch { Code = "GIZ-01", Name = "Giza Branch", Address = "120 Haram Street, Giza", PhoneNumber = "0235678901" }
            );
        }
        #endregion
    }
}
