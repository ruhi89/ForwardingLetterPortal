using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.DTOs.Letter
{
    public class CreateLetterRequestDto
    {
        public string Purpose { get; set; } = null!;
        public string SenderName { get; set; } = null!;
    }

    public class LetterRequestResponseDto
    {
        public int RequestId { get; set; }
        public string RecipientName { get; set; } = null!;
        public string RecipientBankName { get; set; } = null!;
        public string Purpose { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public bool HasGeneratedLetter { get; set; }
        public int DownloadCount { get; set; }
        public DateTime? LastDownloadAt { get; set; }
    }
}
