namespace NationalBank.Models
{
    // Join entity for the "Have" relationship, with its own attributes
    internal class CustomerAccount
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public string AccountNumber { get; set; } = null!;
        public Account Account { get; set; } = null!;

        public OwnershipType OwnershipType { get; set; }
        public DateTime OwnershipStartDate { get; set; }
        public AccountStatus AccountStatus { get; set; }
    }
}
