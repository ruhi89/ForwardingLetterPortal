using System.ComponentModel.DataAnnotations;

namespace ForwardingLetterPortal.Interface.DTOs.Templates
{
    public class TemplateDto
    {
        public int TemplateId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public int PurposeId { get; set; }
        public string PurposeName { get; set; } = null!;
        public string LetterType { get; set; } = null!;
        public string TemplateName { get; set; } = null!;
        public string SubjectText { get; set; } = null!;
        public string BodyText { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SaveTemplateDto
    {
        [Required] public int ServiceId { get; set; }
        [Required] public int PurposeId { get; set; }
        [Required] public string LetterType { get; set; } = null!;
        [Required, StringLength(150)] public string TemplateName { get; set; } = null!;
        [Required, StringLength(500)] public string SubjectText { get; set; } = null!;
        [Required] public string BodyText { get; set; } = null!;
        /// <summary>Activating a template deactivates the current active one for the same service/purpose/letter type.</summary>
        public bool IsActive { get; set; } = true;
    }

    public class PlaceholderDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
