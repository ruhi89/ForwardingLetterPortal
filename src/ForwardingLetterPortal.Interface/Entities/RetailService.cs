namespace ForwardingLetterPortal.Interface.Entities
{
    public class RetailService
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<ServicePurpose> Purposes { get; set; } = [];
    }

    public class ServicePurpose
    {
        public int PurposeId { get; set; }
        public int ServiceId { get; set; }
        public string PurposeName { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public RetailService Service { get; set; } = null!;
    }
}
