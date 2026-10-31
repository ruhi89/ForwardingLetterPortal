using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class LetterRepository(AppDbContext db)
    {
        public async Task<ForwardingLetterRequest> AddRequestAsync(ForwardingLetterRequest request)
        {
            db.ForwardingLetterRequests.Add(request);
            await db.SaveChangesAsync();
            return request;
        }

        public async Task<List<ForwardingLetterRequest>> GetByCustomerAsync(int customerId)
            => await db.ForwardingLetterRequests
                .Include(r => r.GeneratedLetter)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.SubmittedAt)
                .ToListAsync();

        public async Task<ForwardingLetterRequest?> GetByIdAsync(int requestId, int customerId)
            => await db.ForwardingLetterRequests
                .Include(r => r.Customer)
                    .ThenInclude(c => c.BankAccount)
                .Include(r => r.GeneratedLetter)
                .FirstOrDefaultAsync(r => r.RequestId == requestId && r.CustomerId == customerId);

        public async Task SaveLetterAsync(GeneratedLetter letter)
        {
            db.GeneratedLetters.Add(letter);
            await db.SaveChangesAsync();
        }

        public async Task UpdateRequestStatusAsync(int requestId, string status)
        {
            var request = await db.ForwardingLetterRequests.FindAsync(requestId);
            if (request is null) return;
            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
