using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Entities
{
    public class BankAccount
    {
        public int BankAccountId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string BranchName { get; set; } = null!;
        public string AccountType { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public Customer? Customer { get; set; }
    }
}
