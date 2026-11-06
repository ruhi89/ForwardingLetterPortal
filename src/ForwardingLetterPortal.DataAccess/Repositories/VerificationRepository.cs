using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class VerificationRepository(AppDbContext db)
    {
        public Task<BankAccount?> FindAccountAsync(string accountNumber)
            => db.BankAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task AddAsync(OtpVerification verification)
        {
            db.OtpVerifications.Add(verification);
            await db.SaveChangesAsync();
        }

        public Task<OtpVerification?> GetAsync(int verificationId)
            => db.OtpVerifications
                .Include(v => v.BankAccount).ThenInclude(a => a!.Service)
                .FirstOrDefaultAsync(v => v.VerificationId == verificationId);

        public Task SaveAsync() => db.SaveChangesAsync();
    }
}
