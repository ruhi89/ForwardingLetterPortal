namespace ForwardingLetterPortal.Interface.DTOs.Auth
{
    public class VerifyOtpDto
    {
        public string AccountNumber { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }

    public class VerifyOtpResponseDto
    {
        public bool Success { get; set; }
        public string? RegistrationToken { get; set; }
        public string? Message { get; set; }
    }
}
