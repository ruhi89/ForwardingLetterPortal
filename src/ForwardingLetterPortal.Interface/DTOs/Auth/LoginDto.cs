using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.DTOs.Auth
{
    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
