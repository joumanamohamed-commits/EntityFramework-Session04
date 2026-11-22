using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class TransactionConfigurations : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(T => T.TransactionNumber);

            builder.Property(T => T.TransactionDate)
                .IsRequired();

            builder.Property(T => T.Amount)
                .HasPrecision(18, 2);

            builder.Property(T => T.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(T => T.Note)
                .HasMaxLength(250);

            builder.Property(T => T.AccountNumber)
                .IsRequired()
                .HasMaxLength(20);

            #region One-to-Many (Account 1 ---- M Transaction)
            builder.HasOne(T => T.Account)
                .WithMany(A => A.Transactions)
                .HasForeignKey(T => T.AccountNumber)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
