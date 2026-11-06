namespace ForwardingLetterPortal.Interface.Entities
{
    public class ForwardingLetter
    {
        public int LetterId { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string LetterType { get; set; } = null!;
        public int VerificationId { get; set; }
        public int OfficerId { get; set; }
        public int BankAccountId { get; set; }
        public int ServiceId { get; set; }
        public int PurposeId { get; set; }
        public int? TemplateId { get; set; }
        public string SubjectSnapshot { get; set; } = null!;
        public string BodySnapshot { get; set; } = null!;

        public string CustomerName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string? CustomerAddress { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string BranchName { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string PurposeName { get; set; } = null!;

        public string AddressedTo { get; set; } = null!;
        public string? Enclosures { get; set; }
        public string Status { get; set; } = Constants.LetterStatus.Generated;
        public DateTime GeneratedAt { get; set; }
        public DateTime? FirstPrintedAt { get; set; }
        public int PrintCount { get; set; }

        public Officer Officer { get; set; } = null!;
    }

    public class LetterPrintLog
    {
        public int PrintLogId { get; set; }
        public int LetterId { get; set; }
        public int OfficerId { get; set; }
        public string Action { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
