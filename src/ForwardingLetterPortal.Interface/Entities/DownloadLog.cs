using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Entities
{
    public class DownloadLog
    {
        public int LogId { get; set; }
        public int LetterId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DownloadedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public GeneratedLetter Letter { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }
}
