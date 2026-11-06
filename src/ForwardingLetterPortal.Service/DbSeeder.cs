using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.Entities;
using Microsoft.Extensions.Configuration;

namespace ForwardingLetterPortal.Service
{
    public static class DbSeeder
    {
        /// <summary>
        /// Creates the first Super Admin from the "SeedSuperAdmin" config section, but only when the
        /// database has no Super Admin yet. Returns true when an account was created.
        /// </summary>
        public static async Task<bool> SeedSuperAdminAsync(OfficerRepository repo, IConfiguration config)
        {
            var section = config.GetSection("SeedSuperAdmin");
            var employeeId = section["EmployeeId"];
            var email = section["Email"];
            var password = section["Password"];

            if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return false;
            if (await repo.AnySuperAdminAsync())
                return false;

            await repo.AddAsync(new Officer
            {
                EmployeeId = employeeId,
                FullName = section["FullName"] ?? "Super Admin",
                Designation = section["Designation"],
                BranchName = section["BranchName"] ?? "Head Office",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = Roles.SuperAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            return true;
        }
    }
}
