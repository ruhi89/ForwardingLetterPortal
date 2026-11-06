namespace ForwardingLetterPortal.Interface.DTOs.Catalog
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
    }

    public class PurposeDto
    {
        public int PurposeId { get; set; }
        public int ServiceId { get; set; }
        public string PurposeName { get; set; } = null!;
    }
}
