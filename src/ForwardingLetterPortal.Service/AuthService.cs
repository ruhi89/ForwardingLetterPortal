using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Auth;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ForwardingLetterPortal.Service
{
    public class AuthService(OfficerRepository repo, IConfiguration config) : IAuthService
    {
        private const string InvalidCredentials = "Invalid employee ID/e-mail or password.";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto, string? ipAddress)
        {
            var identifier = dto.Identifier.Trim();
            var officer = await repo.FindByIdentifierAsync(identifier);

            if (officer is null)
            {
                await LogAsync(null, identifier, false, "UnknownUser", ipAddress);
                throw AppException.Unauthorized(InvalidCredentials);
            }

            if (!officer.IsActive)
            {
                await LogAsync(officer.OfficerId, identifier, false, "Inactive", ipAddress);
                throw AppException.Unauthorized(InvalidCredentials);
            }

            if (officer.LockedUntil is { } until && until > DateTime.UtcNow)
            {
                await LogAsync(officer.OfficerId, identifier, false, "Locked", ipAddress);
                throw AppException.Unauthorized("Account is temporarily locked. Try again later or contact the Super Admin.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, officer.PasswordHash))
            {
                officer.FailedLoginCount++;
                if (officer.FailedLoginCount >= LoginRules.MaxFailedAttempts)
                {
                    officer.LockedUntil = DateTime.UtcNow.AddMinutes(LoginRules.LockoutMinutes);
                    officer.FailedLoginCount = 0;
                }
                await repo.SaveAsync();
                await LogAsync(officer.OfficerId, identifier, false, "BadPassword", ipAddress);
                throw AppException.Unauthorized(InvalidCredentials);
            }

            officer.FailedLoginCount = 0;
            officer.LockedUntil = null;
            officer.LastLoginAt = DateTime.UtcNow;
            await repo.SaveAsync();
            await LogAsync(officer.OfficerId, identifier, true, null, ipAddress);

            var expires = DateTime.UtcNow.Add(TokenLifetime);
            return new LoginResponseDto
            {
                Token = GenerateToken(officer, expires),
                ExpiresAt = expires,
                OfficerId = officer.OfficerId,
                EmployeeId = officer.EmployeeId,
                FullName = officer.FullName,
                Designation = officer.Designation,
                BranchName = officer.BranchName,
                Role = officer.Role
            };
        }

        public async Task ChangePasswordAsync(int officerId, ChangePasswordDto dto)
        {
            var officer = await repo.GetByIdAsync(officerId)
                ?? throw AppException.NotFound("Officer not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, officer.PasswordHash))
                throw new AppException("Current password is incorrect.");

            officer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            officer.UpdatedAt = DateTime.UtcNow;
            await repo.SaveAsync();
        }

        private Task LogAsync(int? officerId, string identifier, bool success, string? reason, string? ip)
            => repo.AddLoginLogAsync(new OfficerLoginLog
            {
                OfficerId = officerId,
                IdentifierUsed = identifier.Length > 150 ? identifier[..150] : identifier,
                IsSuccess = success,
                FailureReason = reason,
                IpAddress = ip,
                AttemptedAt = DateTime.UtcNow
            });

        private string GenerateToken(Officer officer, DateTime expires)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims:
                [
                    new Claim(ClaimTypes.NameIdentifier, officer.OfficerId.ToString()),
                    new Claim(ClaimTypes.Name, officer.FullName),
                    new Claim(ClaimTypes.Role, officer.Role)
                ],
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
