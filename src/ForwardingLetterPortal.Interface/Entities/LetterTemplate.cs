namespace ForwardingLetterPortal.Interface.Entities
{
    public class LetterTemplate
    {
        public int TemplateId { get; set; }
        public int ServiceId { get; set; }
        public int PurposeId { get; set; }
        public string LetterType { get; set; } = null!;
        public string TemplateName { get; set; } = null!;
        public string SubjectText { get; set; } = null!;
        public string BodyText { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public int? CreatedByOfficerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UpdatedByOfficerId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public RetailService Service { get; set; } = null!;
        public ServicePurpose Purpose { get; set; } = null!;
    }
}
