namespace NationalBank.Models
{
    internal class Branch
    {
        public string Code { get; set; } = null!; // PK (e.g. CAI-01)
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        // Branch 1 ---- 1 Manager (Manage)
        public Manager? Manager { get; set; }

        // Branch 1 ---- M Account (Belong)
        public ICollection<Account> Accounts { get; set; } = new HashSet<Account>();
    }
}
