using ForwardingLetterPortal.Interface.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Interfaces
{
    public interface IAuthService
    {
        Task<VerifyAccountResponseDto> VerifyAccountAsync(VerifyAccountDto dto, string ipAddress);
        Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
        Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
    }
}
