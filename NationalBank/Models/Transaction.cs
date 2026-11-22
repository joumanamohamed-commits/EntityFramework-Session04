namespace NationalBank.Models
{
    internal class Transaction
    {
        public int TransactionNumber { get; set; } // PK
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string? Note { get; set; }

        // Transaction M ---- 1 Account (Related)
        public string AccountNumber { get; set; } = null!;
        public Account Account { get; set; } = null!;
    }
}
