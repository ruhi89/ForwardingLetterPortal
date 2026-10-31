using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.DTOs.Auth
{
    public class RegisterDto
    {
        public string AccountNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string RegistrationToken { get; set; } = null!;
    }
}
