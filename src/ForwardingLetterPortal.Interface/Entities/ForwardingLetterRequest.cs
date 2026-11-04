using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Entities
{
    public class ForwardingLetterRequest
    {
        public int RequestId { get; set; }
        public int CustomerId { get; set; }
        public string SenderName { get; set; } = null!;
        public string SenderAccountNumber { get; set; } = null!;
        public string SenderAddress { get; set; } = null!;
        public string? SenderPhone { get; set; }
        public string? SenderEmail { get; set; }
        public string RecipientName { get; set; } = null!;
        public string RecipientBankName { get; set; } = null!;
        public string RecipientBranchName { get; set; } = null!;
        public string RecipientAddress { get; set; } = null!;
        public string Purpose { get; set; } = null!;
        public string DocumentDescription { get; set; } = null!;
        public string Status { get; set; } = "Submitted";
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer Customer { get; set; } = null!;
        public GeneratedLetter? GeneratedLetter { get; set; }
    }
}
