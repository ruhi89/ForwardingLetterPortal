using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class AuthRepository(AppDbContext db)
    {
        public async Task<BankAccount?> FindBankAccountAsync(string accountNumber)
            => await db.BankAccounts.FirstOrDefaultAsync(b => b.AccountNumber == accountNumber && b.IsActive);

        public async Task<bool> IsAlreadyRegisteredAsync(int bankAccountId)
            => await db.Customers.AnyAsync(c => c.BankAccountId == bankAccountId);

        public async Task<bool> IsUsernameTakenAsync(string username)
            => await db.Customers.AnyAsync(c => c.Username == username);

        public async Task AddCustomerAsync(Customer customer)
        {
            db.Customers.Add(customer);
            await db.SaveChangesAsync();
        }

        public async Task<Customer?> FindCustomerByUsernameAsync(string username)
            => await db.Customers
                .Include(c => c.BankAccount)
                .FirstOrDefaultAsync(c => c.Username == username && c.IsActive);
    }
}
