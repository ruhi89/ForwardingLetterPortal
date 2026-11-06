using ForwardingLetterPortal.Interface.DTOs.Auth;
using ForwardingLetterPortal.Interface.DTOs.Catalog;
using ForwardingLetterPortal.Interface.DTOs.Common;
using ForwardingLetterPortal.Interface.DTOs.Letters;
using ForwardingLetterPortal.Interface.DTOs.Officers;
using ForwardingLetterPortal.Interface.DTOs.Templates;
using ForwardingLetterPortal.Interface.DTOs.Verification;

namespace ForwardingLetterPortal.Interface.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto, string? ipAddress);
        Task ChangePasswordAsync(int officerId, ChangePasswordDto dto);
    }

    public interface IOfficerService
    {
        Task<List<OfficerDto>> ListAsync();
        Task<OfficerDto> GetAsync(int officerId);
        Task<OfficerDto> CreateAsync(int actingOfficerId, CreateOfficerDto dto);
        Task<OfficerDto> UpdateAsync(int actingOfficerId, int officerId, UpdateOfficerDto dto);
        Task ResetPasswordAsync(int officerId, ResetPasswordDto dto);
        Task UnlockAsync(int officerId);
    }

    public interface ICatalogService
    {
        Task<List<ServiceDto>> GetServicesAsync();
        Task<List<PurposeDto>> GetPurposesAsync(int serviceId);
    }

    public interface ITemplateService
    {
        List<PlaceholderDto> GetPlaceholders();
        Task<List<TemplateDto>> ListAsync(int? serviceId, int? purposeId, string? letterType, bool? isActive);
        Task<TemplateDto> GetAsync(int templateId);
        Task<TemplateDto> CreateAsync(int actingOfficerId, SaveTemplateDto dto);
        Task<TemplateDto> UpdateAsync(int actingOfficerId, int templateId, SaveTemplateDto dto);
        Task<TemplateDto> SetActiveAsync(int actingOfficerId, int templateId, bool isActive);
    }

    public interface IVerificationService
    {
        Task<StartVerificationResponseDto> StartAsync(int officerId, StartVerificationDto dto, string? ipAddress);
        Task<ConfirmOtpResponseDto> ConfirmAsync(int officerId, int verificationId, ConfirmOtpDto dto);
        Task<CustomerInfoDto> GetVerifiedCustomerAsync(int officerId, int verificationId);
    }

    public interface ILetterService
    {
        Task<LetterDto> GenerateAsync(int officerId, GenerateLetterDto dto);
        Task<PagedResult<LetterListItemDto>> ListAsync(int officerId, bool isSuperAdmin, LetterQueryDto query);
        Task<LetterDto> GetAsync(int officerId, bool isSuperAdmin, int letterId);
        /// <summary>Renders the stored letter as PDF and records a Preview or Print action.</summary>
        Task<byte[]> GetPdfAsync(int officerId, bool isSuperAdmin, int letterId, string action, string? ipAddress);
    }
}
