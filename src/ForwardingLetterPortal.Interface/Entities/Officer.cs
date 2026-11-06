namespace ForwardingLetterPortal.Interface.Entities
{
    public class Officer
    {
        public int OfficerId { get; set; }
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Designation { get; set; }
        public string BranchName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = Constants.Roles.Officer;
        public bool IsActive { get; set; } = true;
        public int FailedLoginCount { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int? CreatedByOfficerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class OfficerLoginLog
    {
        public int LoginLogId { get; set; }
        public int? OfficerId { get; set; }
        public string IdentifierUsed { get; set; } = null!;
        public bool IsSuccess { get; set; }
        public string? FailureReason { get; set; }
        public string? IpAddress { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
