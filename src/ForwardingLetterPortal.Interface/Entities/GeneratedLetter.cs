using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Entities
{
    public class GeneratedLetter
    {
        public int LetterId { get; set; }
        public int RequestId { get; set; }
        public int CustomerId { get; set; }
        public string FilePath { get; set; } = null!;
        public long? FileSize { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int DownloadCount { get; set; }
        public DateTime? LastDownloadAt { get; set; }

        public ForwardingLetterRequest Request { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
    }
}
