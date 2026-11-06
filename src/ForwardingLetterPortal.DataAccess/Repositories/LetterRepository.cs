using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class LetterRepository(AppDbContext db)
    {
        public Task<int> NextReferenceSequenceAsync()
            => db.Database.SqlQuery<int>($"SELECT NEXT VALUE FOR dbo.LetterReferenceSeq AS [Value]").FirstAsync();

        public async Task AddAsync(ForwardingLetter letter)
        {
            db.ForwardingLetters.Add(letter);
            await db.SaveChangesAsync();
        }

        public Task<ForwardingLetter?> GetAsync(int letterId)
            => db.ForwardingLetters.Include(l => l.Officer).FirstOrDefaultAsync(l => l.LetterId == letterId);

        public IQueryable<ForwardingLetter> Query()
            => db.ForwardingLetters.AsNoTracking().Include(l => l.Officer);

        public async Task AddPrintLogAsync(LetterPrintLog log)
        {
            db.LetterPrintLogs.Add(log);
            await db.SaveChangesAsync();
        }

        public Task SaveAsync() => db.SaveChangesAsync();
    }
}
