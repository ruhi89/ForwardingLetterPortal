using System.ComponentModel.DataAnnotations;

namespace ForwardingLetterPortal.Interface.DTOs.Verification
{
    public class StartVerificationDto
    {
        [Required, StringLength(20)] public string AccountNumber { get; set; } = null!;
        /// <summary>Customer's registered e-mail address; the OTP is sent here.</summary>
        [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = null!;
    }

    public class StartVerificationResponseDto
    {
        public bool Success { get; set; }
        public int? VerificationId { get; set; }
        public string? Channel { get; set; }
        public string? MaskedContact { get; set; }
        public DateTime? OtpExpiresAt { get; set; }
        public string Message { get; set; } = null!;
    }

    public class ConfirmOtpDto
    {
        [Required, StringLength(10, MinimumLength = 4)] public string Otp { get; set; } = null!;
    }

    public class ConfirmOtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public CustomerInfoDto? Customer { get; set; }
    }

    public class CustomerInfoDto
    {
        public string AccountNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string BranchName { get; set; } = null!;
        public string? ServiceName { get; set; }
        public DateTime? OpenedOn { get; set; }
        public string AccountStatus { get; set; } = null!;
    }
}
