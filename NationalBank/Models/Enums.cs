namespace NationalBank.Models
{
    internal enum CustomerType
    {
        Individual = 1,
        Business = 2
    }

    internal enum AccountType
    {
        Savings = 1,
        Current = 2,
        Business = 3
    }

    internal enum OwnershipType
    {
        Primary = 1,
        CoHolder = 2
    }

    internal enum AccountStatus
    {
        Active = 1,
        Closed = 2
    }

    internal enum TransactionType
    {
        Deposit = 1,
        Withdrawal = 2,
        Transfer = 3
    }
}
