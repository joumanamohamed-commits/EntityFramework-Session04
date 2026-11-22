using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class AccountConfigurations : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(A => A.AccountNumber);

            builder.Property(A => A.AccountNumber)
                .HasMaxLength(20);

            builder.Property(A => A.CurrentBalance)
                .HasPrecision(18, 2);

            builder.Property(A => A.AccountType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(A => A.OpeningDate)
                .IsRequired();

            builder.Property(A => A.BranchCode)
                .IsRequired()
                .HasMaxLength(10);

            #region One-to-Many (Branch 1 ---- M Account)
            builder.HasOne(A => A.Branch)
                .WithMany(B => B.Accounts)
                .HasForeignKey(A => A.BranchCode)
                .OnDelete(DeleteBehavior.Restrict); // a branch that still has accounts cannot be deleted
            #endregion
        }
    }
}
