using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public int BankAccountId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public BankAccount BankAccount { get; set; } = null!;
        public ICollection<ForwardingLetterRequest> Requests { get; set; } = [];
    }
}
