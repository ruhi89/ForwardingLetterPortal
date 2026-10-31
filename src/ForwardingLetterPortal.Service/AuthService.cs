using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.DTOs.Auth;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ForwardingLetterPortal.Service
{
    public class AuthService(AuthRepository repo, IConfiguration config, IMemoryCache cache, IEmailService email) : IAuthService
    {
        private const int MaxOtpAttempts = 5;
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
        private const string RegisterPurpose = "register";

        private sealed class OtpEntry
        {
            public string Hash { get; init; } = null!;
            public int Attempts { get; set; }
        }

        private static string OtpKey(string accountNumber) => $"otp:{accountNumber}";

        public async Task<VerifyAccountResponseDto> VerifyAccountAsync(VerifyAccountDto dto, string ipAddress)
        {
            var account = await repo.FindBankAccountAsync(dto.AccountNumber);

            if (account is null)
                return new VerifyAccountResponseDto { Success = false, Message = "Account not found." };

            if (!account.Email.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
                return new VerifyAccountResponseDto { Success = false, Message = "Email does not match." };

            if (account.DateOfBirth.Date != dto.DateOfBirth.Date)
                return new VerifyAccountResponseDto { Success = false, Message = "Date of birth does not match." };

            if (await repo.IsAlreadyRegisteredAsync(account.BankAccountId))
                return new VerifyAccountResponseDto { Success = false, Message = "This account is already registered." };

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            cache.Set(OtpKey(account.AccountNumber),
                new OtpEntry { Hash = BCrypt.Net.BCrypt.HashPassword(otp) }, OtpLifetime);

            await email.SendAsync(account.Email, "Your verification code",
                $"<p>Your Forwarding Letter Portal verification code is <b>{otp}</b>.</p>" +
                $"<p>It expires in {OtpLifetime.TotalMinutes:0} minutes. Do not share it with anyone.</p>");

            return new VerifyAccountResponseDto
            {
                Success = true,
                FullName = account.FullName,
                BranchName = account.BranchName,
                Message = "A verification code has been sent to your registered email."
            };
        }

        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var key = OtpKey(dto.AccountNumber);

            if (!cache.TryGetValue(key, out OtpEntry? entry) || entry is null)
                return new VerifyOtpResponseDto { Success = false, Message = "Code expired or not requested. Please verify your account again." };

            if (++entry.Attempts > MaxOtpAttempts)
            {
                cache.Remove(key);
                return new VerifyOtpResponseDto { Success = false, Message = "Too many attempts. Please verify your account again." };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Otp, entry.Hash))
                return new VerifyOtpResponseDto { Success = false, Message = "Invalid code." };

            cache.Remove(key);

            var account = await repo.FindBankAccountAsync(dto.AccountNumber);
            if (account is null)
                return new VerifyOtpResponseDto { Success = false, Message = "Account not found." };

            return new VerifyOtpResponseDto
            {
                Success = true,
                RegistrationToken = GenerateRegistrationToken(account.AccountNumber)
            };
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (!ValidateRegistrationToken(dto.RegistrationToken, dto.AccountNumber))
                throw new Exception("Email verification required or expired. Please verify again.");

            var account = await repo.FindBankAccountAsync(dto.AccountNumber)
                ?? throw new Exception("Account not found.");

            if (await repo.IsAlreadyRegisteredAsync(account.BankAccountId))
                throw new Exception("This account is already registered.");

            if (await repo.IsUsernameTakenAsync(dto.Username))
                throw new Exception("Username is already taken.");

            var customer = new Customer
            {
                BankAccountId = account.BankAccountId,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddCustomerAsync(customer);

            return new LoginResponseDto
            {
                Token = GenerateToken(customer.CustomerId, dto.Username),
                FullName = account.FullName,
                Username = dto.Username
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var customer = await repo.FindCustomerByUsernameAsync(dto.Username)
                ?? throw new Exception("Invalid username or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.PasswordHash))
                throw new Exception("Invalid username or password.");

            return new LoginResponseDto
            {
                Token = GenerateToken(customer.CustomerId, customer.Username),
                FullName = customer.BankAccount.FullName,
                Username = customer.Username
            };
        }

        private string GenerateRegistrationToken(string accountNumber)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims:
                [
                    new Claim("purpose", RegisterPurpose),
                    new Claim("account", accountNumber)
                ],
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool ValidateRegistrationToken(string? token, string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal.FindFirst("purpose")?.Value == RegisterPurpose
                    && principal.FindFirst("account")?.Value == accountNumber;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateToken(int customerId, string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
