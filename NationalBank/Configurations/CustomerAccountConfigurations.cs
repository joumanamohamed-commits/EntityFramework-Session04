using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class CustomerAccountConfigurations : IEntityTypeConfiguration<CustomerAccount>
    {
        public void Configure(EntityTypeBuilder<CustomerAccount> builder)
        {
            
            builder.HasKey(CA => new { CA.CustomerId, CA.AccountNumber });

            builder.Property(CA => CA.AccountNumber)
                .HasMaxLength(20);

            builder.Property(CA => CA.OwnershipType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(CA => CA.AccountStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(CA => CA.OwnershipStartDate)
                .IsRequired();

            #region Many-to-Many (Customer M ---- M Account)
            builder.HasOne(CA => CA.Customer)
                .WithMany(C => C.CustomerAccounts)
                .HasForeignKey(CA => CA.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(CA => CA.Account)
                .WithMany(A => A.CustomerAccounts)
                .HasForeignKey(CA => CA.AccountNumber)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
