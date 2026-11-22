using Microsoft.EntityFrameworkCore;
using NationalBank.Configurations;
using NationalBank.Models;

namespace NationalBank
{
    internal class APPDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;DataBase=NationalBankDb;Trusted_Connection=true;TrustServerCertificate=true");
        }

        #region DbSets
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Branch>(new BranchConfigurations());
            modelBuilder.ApplyConfiguration<Manager>(new ManagerConfigurations());
            modelBuilder.ApplyConfiguration<Customer>(new CustomerConfigurations());
            modelBuilder.ApplyConfiguration<Account>(new AccountConfigurations());
            modelBuilder.ApplyConfiguration<CustomerAccount>(new CustomerAccountConfigurations());
            modelBuilder.ApplyConfiguration<Transaction>(new TransactionConfigurations());
        }
    }
}
