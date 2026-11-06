namespace ForwardingLetterPortal.Interface.Entities
{
    public class OtpVerification
    {
        public int VerificationId { get; set; }
        public int OfficerId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public int? BankAccountId { get; set; }
        public string ContactEntered { get; set; } = null!;
        public string? Channel { get; set; }
        public string? OtpHash { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public int AttemptCount { get; set; }
        public string Status { get; set; } = Constants.OtpStatus.Pending;
        public string? FailureReason { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public BankAccount? BankAccount { get; set; }
    }
}
