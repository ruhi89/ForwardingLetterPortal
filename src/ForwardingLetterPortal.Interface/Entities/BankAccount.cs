namespace ForwardingLetterPortal.Interface.Entities
{
    /// <summary>Read-only mirror of core banking records.</summary>
    public class BankAccount
    {
        public int BankAccountId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string BranchName { get; set; } = null!;
        public int? ServiceId { get; set; }
        public DateTime? OpenedOn { get; set; }
        public string AccountStatus { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public RetailService? Service { get; set; }
    }
}
