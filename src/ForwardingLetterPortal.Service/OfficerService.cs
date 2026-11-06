using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Officers;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;

namespace ForwardingLetterPortal.Service
{
    public class OfficerService(OfficerRepository repo) : IOfficerService
    {
        public async Task<List<OfficerDto>> ListAsync()
            => (await repo.ListAsync()).Select(Map).ToList();

        public async Task<OfficerDto> GetAsync(int officerId)
            => Map(await repo.GetByIdAsync(officerId) ?? throw AppException.NotFound("Officer not found."));

        public async Task<OfficerDto> CreateAsync(int actingOfficerId, CreateOfficerDto dto)
        {
            var employeeId = dto.EmployeeId.Trim();
            var email = dto.Email.Trim();
            EnsureValidRole(dto.Role);

            if (await repo.EmployeeIdExistsAsync(employeeId))
                throw AppException.Conflict("Employee ID is already in use.");
            if (await repo.EmailExistsAsync(email))
                throw AppException.Conflict("E-mail is already in use.");

            var officer = new Officer
            {
                EmployeeId = employeeId,
                FullName = dto.FullName.Trim(),
                Designation = dto.Designation?.Trim(),
                BranchName = dto.BranchName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsActive = true,
                CreatedByOfficerId = actingOfficerId,
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(officer);
            return Map(officer);
        }

        public async Task<OfficerDto> UpdateAsync(int actingOfficerId, int officerId, UpdateOfficerDto dto)
        {
            EnsureValidRole(dto.Role);
            var officer = await repo.GetByIdAsync(officerId)
                ?? throw AppException.NotFound("Officer not found.");

            var email = dto.Email.Trim();
            if (await repo.EmailExistsAsync(email, officerId))
                throw AppException.Conflict("E-mail is already in use.");

            var losesAdminAccess = officer.Role == Roles.SuperAdmin && officer.IsActive
                                   && (dto.Role != Roles.SuperAdmin || !dto.IsActive);
            if (losesAdminAccess)
            {
                if (officerId == actingOfficerId)
                    throw new AppException("You cannot demote or deactivate your own account.");
                if (!await repo.AnotherActiveSuperAdminExistsAsync(officerId))
                    throw new AppException("At least one active Super Admin must remain.");
            }

            officer.FullName = dto.FullName.Trim();
            officer.Designation = dto.Designation?.Trim();
            officer.BranchName = dto.BranchName.Trim();
            officer.Email = email;
            officer.Role = dto.Role;
            officer.IsActive = dto.IsActive;
            officer.UpdatedAt = DateTime.UtcNow;
            await repo.SaveAsync();
            return Map(officer);
        }

        public async Task ResetPasswordAsync(int officerId, ResetPasswordDto dto)
        {
            var officer = await repo.GetByIdAsync(officerId)
                ?? throw AppException.NotFound("Officer not found.");
            officer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            officer.FailedLoginCount = 0;
            officer.LockedUntil = null;
            officer.UpdatedAt = DateTime.UtcNow;
            await repo.SaveAsync();
        }

        public async Task UnlockAsync(int officerId)
        {
            var officer = await repo.GetByIdAsync(officerId)
                ?? throw AppException.NotFound("Officer not found.");
            officer.FailedLoginCount = 0;
            officer.LockedUntil = null;
            officer.UpdatedAt = DateTime.UtcNow;
            await repo.SaveAsync();
        }

        private static void EnsureValidRole(string role)
        {
            if (!Roles.All.Contains(role))
                throw new AppException($"Role must be one of: {string.Join(", ", Roles.All)}.");
        }

        private static OfficerDto Map(Officer o) => new()
        {
            OfficerId = o.OfficerId,
            EmployeeId = o.EmployeeId,
            FullName = o.FullName,
            Designation = o.Designation,
            BranchName = o.BranchName,
            Email = o.Email,
            Role = o.Role,
            IsActive = o.IsActive,
            IsLocked = o.LockedUntil > DateTime.UtcNow,
            CreatedAt = o.CreatedAt,
            LastLoginAt = o.LastLoginAt
        };
    }
}
