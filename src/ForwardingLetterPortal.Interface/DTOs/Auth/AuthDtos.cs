using System.ComponentModel.DataAnnotations;

namespace ForwardingLetterPortal.Interface.DTOs.Auth
{
    public class LoginDto
    {
        /// <summary>Employee ID or e-mail address.</summary>
        [Required] public string Identifier { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public int OfficerId { get; set; }
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Designation { get; set; }
        public string BranchName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class ChangePasswordDto
    {
        [Required] public string CurrentPassword { get; set; } = null!;
        [Required, MinLength(8)] public string NewPassword { get; set; } = null!;
    }
}
