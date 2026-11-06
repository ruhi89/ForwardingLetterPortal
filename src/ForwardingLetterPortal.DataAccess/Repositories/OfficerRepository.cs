using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class OfficerRepository(AppDbContext db)
    {
        public Task<Officer?> FindByIdentifierAsync(string identifier)
            => db.Officers.FirstOrDefaultAsync(o => o.EmployeeId == identifier || o.Email == identifier);

        public Task<Officer?> GetByIdAsync(int officerId)
            => db.Officers.FirstOrDefaultAsync(o => o.OfficerId == officerId);

        public Task<List<Officer>> ListAsync()
            => db.Officers.AsNoTracking().OrderBy(o => o.FullName).ToListAsync();

        public Task<bool> EmployeeIdExistsAsync(string employeeId)
            => db.Officers.AnyAsync(o => o.EmployeeId == employeeId);

        public Task<bool> EmailExistsAsync(string email, int? excludeOfficerId = null)
            => db.Officers.AnyAsync(o => o.Email == email && o.OfficerId != excludeOfficerId);

        /// <summary>Active Super Admins other than the given officer (guards against locking everyone out).</summary>
        public Task<bool> AnotherActiveSuperAdminExistsAsync(int officerId)
            => db.Officers.AnyAsync(o => o.Role == Roles.SuperAdmin && o.IsActive && o.OfficerId != officerId);

        public Task<bool> AnySuperAdminAsync()
            => db.Officers.AnyAsync(o => o.Role == Roles.SuperAdmin);

        public async Task AddAsync(Officer officer)
        {
            db.Officers.Add(officer);
            await db.SaveChangesAsync();
        }

        public async Task AddLoginLogAsync(OfficerLoginLog log)
        {
            db.OfficerLoginLogs.Add(log);
            await db.SaveChangesAsync();
        }

        public Task SaveAsync() => db.SaveChangesAsync();
    }
}
