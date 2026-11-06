using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Templates;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;

namespace ForwardingLetterPortal.Service
{
    public class TemplateService(CatalogRepository repo) : ITemplateService
    {
        public List<PlaceholderDto> GetPlaceholders() => TemplateRenderer.Describe();

        public async Task<List<TemplateDto>> ListAsync(int? serviceId, int? purposeId, string? letterType, bool? isActive)
            => (await repo.ListTemplatesAsync(serviceId, purposeId, letterType, isActive)).Select(Map).ToList();

        public async Task<TemplateDto> GetAsync(int templateId)
            => Map(await repo.GetTemplateAsync(templateId) ?? throw AppException.NotFound("Template not found."));

        public async Task<TemplateDto> CreateAsync(int actingOfficerId, SaveTemplateDto dto)
        {
            await ValidateAsync(dto);

            var template = new LetterTemplate
            {
                ServiceId = dto.ServiceId,
                PurposeId = dto.PurposeId,
                LetterType = dto.LetterType,
                TemplateName = dto.TemplateName.Trim(),
                SubjectText = dto.SubjectText.Trim(),
                BodyText = dto.BodyText.Trim(),
                IsActive = dto.IsActive,
                CreatedByOfficerId = actingOfficerId,
                CreatedAt = DateTime.UtcNow
            };

            await repo.SaveTemplateAsync(template, actingOfficerId);
            return await GetAsync(template.TemplateId);
        }

        public async Task<TemplateDto> UpdateAsync(int actingOfficerId, int templateId, SaveTemplateDto dto)
        {
            await ValidateAsync(dto);
            var template = await repo.GetTemplateAsync(templateId)
                ?? throw AppException.NotFound("Template not found.");

            template.ServiceId = dto.ServiceId;
            template.PurposeId = dto.PurposeId;
            template.LetterType = dto.LetterType;
            template.TemplateName = dto.TemplateName.Trim();
            template.SubjectText = dto.SubjectText.Trim();
            template.BodyText = dto.BodyText.Trim();
            template.IsActive = dto.IsActive;
            template.UpdatedByOfficerId = actingOfficerId;
            template.UpdatedAt = DateTime.UtcNow;

            await repo.SaveTemplateAsync(template, actingOfficerId);
            return await GetAsync(templateId);
        }

        public async Task<TemplateDto> SetActiveAsync(int actingOfficerId, int templateId, bool isActive)
        {
            var template = await repo.GetTemplateAsync(templateId)
                ?? throw AppException.NotFound("Template not found.");

            template.IsActive = isActive;
            template.UpdatedByOfficerId = actingOfficerId;
            template.UpdatedAt = DateTime.UtcNow;

            await repo.SaveTemplateAsync(template, actingOfficerId);
            return Map(template);
        }

        private async Task ValidateAsync(SaveTemplateDto dto)
        {
            if (!LetterTypes.IsValid(dto.LetterType))
                throw new AppException($"LetterType must be {LetterTypes.CustomerCopy} or {LetterTypes.InternalCopy}.");

            var service = await repo.GetServiceAsync(dto.ServiceId);
            if (service is null) throw new AppException("Service not found.");

            var purpose = await repo.GetPurposeAsync(dto.PurposeId);
            if (purpose is null || purpose.ServiceId != dto.ServiceId)
                throw new AppException("Purpose does not belong to the selected service.");

            var unknown = TemplateRenderer.FindUnknown(dto.SubjectText)
                .Concat(TemplateRenderer.FindUnknown(dto.BodyText)).Distinct().ToList();
            if (unknown.Count > 0)
                throw new AppException("Unknown placeholder(s): " + string.Join(", ", unknown.Select(u => "{{" + u + "}}")));
        }

        private static TemplateDto Map(LetterTemplate t) => new()
        {
            TemplateId = t.TemplateId,
            ServiceId = t.ServiceId,
            ServiceName = t.Service.ServiceName,
            PurposeId = t.PurposeId,
            PurposeName = t.Purpose.PurposeName,
            LetterType = t.LetterType,
            TemplateName = t.TemplateName,
            SubjectText = t.SubjectText,
            BodyText = t.BodyText,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}
