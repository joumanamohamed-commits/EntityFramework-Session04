namespace NationalBank.Models
{
    internal class Account
    {
        public string AccountNumber { get; set; } = null!; // PK
        public decimal CurrentBalance { get; set; }
        public AccountType AccountType { get; set; }
        public DateTime OpeningDate { get; set; }

        // Account M ---- 1 Branch (Belong)
        public string BranchCode { get; set; } = null!;
        public Branch Branch { get; set; } = null!;

        // Account M ---- M Customer (Have) => joint accounts
        public ICollection<CustomerAccount> CustomerAccounts { get; set; } = new HashSet<CustomerAccount>();

        // Account 1 ---- M Transaction (Related)
        public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
