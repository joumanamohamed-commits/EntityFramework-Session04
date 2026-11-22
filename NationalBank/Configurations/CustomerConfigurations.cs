using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Configurations
{
    internal class CustomerConfigurations : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(C => C.Id);

            builder.Property(C => C.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(C => C.NationalId)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(C => C.NationalId)
                .IsUnique(); 

            builder.Property(C => C.DateOfBirth)
                .HasColumnType("date");

            builder.Property(C => C.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(C => C.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(C => C.Address)
                .IsRequired()
                .HasMaxLength(200);

            
            builder.Property(C => C.CustomerType)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}
