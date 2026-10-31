using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.DTOs.Auth
{
    public class VerifyAccountDto
    {
        public string AccountNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
    }

    public class VerifyAccountResponseDto
    {
        public bool Success { get; set; }
        public string? FullName { get; set; }
        public string? BranchName { get; set; }
        public string? Message { get; set; }
    }
}
