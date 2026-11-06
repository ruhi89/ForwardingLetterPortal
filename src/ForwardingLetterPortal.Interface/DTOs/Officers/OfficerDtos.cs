using System.ComponentModel.DataAnnotations;

namespace ForwardingLetterPortal.Interface.DTOs.Officers
{
    public class OfficerDto
    {
        public int OfficerId { get; set; }
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Designation { get; set; }
        public string BranchName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateOfficerDto
    {
        [Required, StringLength(30)] public string EmployeeId { get; set; } = null!;
        [Required, StringLength(150)] public string FullName { get; set; } = null!;
        [StringLength(100)] public string? Designation { get; set; }
        [Required, StringLength(200)] public string BranchName { get; set; } = null!;
        [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = null!;
        [Required, MinLength(8)] public string Password { get; set; } = null!;
        public string Role { get; set; } = Constants.Roles.Officer;
    }

    public class UpdateOfficerDto
    {
        [Required, StringLength(150)] public string FullName { get; set; } = null!;
        [StringLength(100)] public string? Designation { get; set; }
        [Required, StringLength(200)] public string BranchName { get; set; } = null!;
        [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = null!;
        [Required] public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordDto
    {
        [Required, MinLength(8)] public string NewPassword { get; set; } = null!;
    }
}
