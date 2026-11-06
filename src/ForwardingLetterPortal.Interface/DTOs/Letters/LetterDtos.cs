using System.ComponentModel.DataAnnotations;

namespace ForwardingLetterPortal.Interface.DTOs.Letters
{
    public class GenerateLetterDto
    {
        [Required] public int VerificationId { get; set; }
        [Required] public int ServiceId { get; set; }
        [Required] public int PurposeId { get; set; }
        /// <summary>CustomerCopy | InternalCopy</summary>
        [Required] public string LetterType { get; set; } = null!;
        /// <summary>Supporting documents, one per line. InternalCopy only.</summary>
        [StringLength(1000)] public string? Enclosures { get; set; }
    }

    public class LetterDto
    {
        public int LetterId { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string LetterType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string? CustomerAddress { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string BranchName { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string PurposeName { get; set; } = null!;
        public int? TemplateId { get; set; }
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string AddressedTo { get; set; } = null!;
        public string? Enclosures { get; set; }
        public int OfficerId { get; set; }
        public string OfficerName { get; set; } = null!;
        public string OfficerEmployeeId { get; set; } = null!;
        public DateTime GeneratedAt { get; set; }
        public DateTime? FirstPrintedAt { get; set; }
        public int PrintCount { get; set; }
    }

    public class LetterListItemDto
    {
        public int LetterId { get; set; }
        public string ReferenceNumber { get; set; } = null!;
        public string LetterType { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string PurposeName { get; set; } = null!;
        public string OfficerName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int PrintCount { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class LetterQueryDto
    {
        /// <summary>Matches reference number, account number or customer name.</summary>
        public string? Search { get; set; }
        public string? LetterType { get; set; }
        public int? ServiceId { get; set; }
        /// <summary>Honoured for SuperAdmin only; officers always see their own letters.</summary>
        public int? OfficerId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
