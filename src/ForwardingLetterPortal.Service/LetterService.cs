using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Common;
using ForwardingLetterPortal.Interface.DTOs.Letters;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.Service
{
    public class LetterService(
        LetterRepository letters,
        VerificationRepository verifications,
        CatalogRepository catalog,
        OfficerRepository officers) : ILetterService
    {
        // Both letter types are addressed to the MD & CEO.
        private const string DefaultAddressedTo = "The Managing Director & CEO\nAgrani Bank PLC.\nHead Office, 9/D Dilkusha C/A, Dhaka-1000";

        public async Task<LetterDto> GenerateAsync(int officerId, GenerateLetterDto dto)
        {
            if (!LetterTypes.IsValid(dto.LetterType))
                throw new AppException($"LetterType must be {LetterTypes.CustomerCopy} or {LetterTypes.InternalCopy}.");

            // Step 6/7 must be complete: a verified, unexpired OTP session owned by this officer.
            var verification = await verifications.GetAsync(dto.VerificationId);
            if (verification is null || verification.OfficerId != officerId)
                throw AppException.NotFound("Verification not found.");
            if (!VerificationService.IsUsable(verification))
                throw new AppException("Customer verification is missing or has expired. Please verify again.");
            var account = verification.BankAccount!;

            var service = await catalog.GetServiceAsync(dto.ServiceId);
            if (service is null || !service.IsActive) throw new AppException("Service not found.");
            var purpose = await catalog.GetPurposeAsync(dto.PurposeId);
            if (purpose is null || !purpose.IsActive || purpose.ServiceId != dto.ServiceId)
                throw new AppException("Purpose does not belong to the selected service.");

            var template = await catalog.FindActiveTemplateAsync(dto.ServiceId, dto.PurposeId, dto.LetterType)
                ?? throw new AppException("No active letter template is configured for this service, purpose and letter type. Please contact the Super Admin.");

            var officer = await officers.GetByIdAsync(officerId)
                ?? throw AppException.NotFound("Officer not found.");

            var enclosures = string.IsNullOrWhiteSpace(dto.Enclosures) ? null : dto.Enclosures.Trim();
            if (dto.LetterType == LetterTypes.CustomerCopy && enclosures is not null)
                throw new AppException("Enclosures are only allowed on the Internal Forwarding Copy.");

            var now = DateTime.UtcNow;
            var seq = await letters.NextReferenceSequenceAsync();
            var reference = $"AGR/RET/{now.Year}/{seq:D6}";

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CustomerName"] = account.FullName,
                ["FatherName"] = account.FatherName ?? "",
                ["AccountNumber"] = account.AccountNumber,
                ["CustomerAddress"] = account.Address ?? "",
                ["CustomerMobile"] = account.MobileNumber ?? "",
                ["CustomerEmail"] = account.Email ?? "",
                ["BranchName"] = account.BranchName,
                ["ServiceName"] = service.ServiceName,
                ["PurposeName"] = purpose.PurposeName,
                ["ReferenceNumber"] = reference,
                ["Date"] = now.AddHours(6).ToString("dd MMMM yyyy"),
                ["OfficerName"] = officer.FullName,
                ["OfficerDesignation"] = officer.Designation ?? "",
                ["OfficerBranch"] = officer.BranchName
            };

            var letter = new ForwardingLetter
            {
                ReferenceNumber = reference,
                LetterType = dto.LetterType,
                VerificationId = verification.VerificationId,
                OfficerId = officerId,
                BankAccountId = account.BankAccountId,
                ServiceId = service.ServiceId,
                PurposeId = purpose.PurposeId,
                TemplateId = template.TemplateId,
                SubjectSnapshot = TemplateRenderer.Render(template.SubjectText, values),
                BodySnapshot = TemplateRenderer.Render(template.BodyText, values),
                CustomerName = account.FullName,
                AccountNumber = account.AccountNumber,
                CustomerAddress = account.Address,
                CustomerMobile = account.MobileNumber,
                CustomerEmail = account.Email,
                BranchName = account.BranchName,
                ServiceName = service.ServiceName,
                PurposeName = purpose.PurposeName,
                AddressedTo = DefaultAddressedTo,
                Enclosures = enclosures,
                Status = LetterStatus.Generated,
                GeneratedAt = now,
                PrintCount = 0
            };

            await letters.AddAsync(letter);
            letter.Officer = officer;
            return Map(letter);
        }

        public async Task<PagedResult<LetterListItemDto>> ListAsync(int officerId, bool isSuperAdmin, LetterQueryDto q)
        {
            var page = Math.Max(1, q.Page);
            var pageSize = Math.Clamp(q.PageSize, 1, 100);

            var query = letters.Query();

            if (isSuperAdmin)
            {
                if (q.OfficerId is { } oid) query = query.Where(l => l.OfficerId == oid);
            }
            else
            {
                query = query.Where(l => l.OfficerId == officerId);
            }

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(l => l.ReferenceNumber.Contains(s)
                                      || l.AccountNumber.Contains(s)
                                      || l.CustomerName.Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(q.LetterType)) query = query.Where(l => l.LetterType == q.LetterType);
            if (q.ServiceId is { } sid) query = query.Where(l => l.ServiceId == sid);
            if (q.From is { } from) query = query.Where(l => l.GeneratedAt >= from.Date);
            if (q.To is { } to) query = query.Where(l => l.GeneratedAt < to.Date.AddDays(1));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.GeneratedAt).ThenByDescending(l => l.LetterId)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(l => new LetterListItemDto
                {
                    LetterId = l.LetterId,
                    ReferenceNumber = l.ReferenceNumber,
                    LetterType = l.LetterType,
                    CustomerName = l.CustomerName,
                    AccountNumber = l.AccountNumber,
                    ServiceName = l.ServiceName,
                    PurposeName = l.PurposeName,
                    OfficerName = l.Officer.FullName,
                    Status = l.Status,
                    PrintCount = l.PrintCount,
                    GeneratedAt = l.GeneratedAt
                })
                .ToListAsync();

            return new PagedResult<LetterListItemDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
        }

        public async Task<LetterDto> GetAsync(int officerId, bool isSuperAdmin, int letterId)
            => Map(await GetAccessibleAsync(officerId, isSuperAdmin, letterId));

        public async Task<byte[]> GetPdfAsync(int officerId, bool isSuperAdmin, int letterId, string action, string? ipAddress)
        {
            var letter = await GetAccessibleAsync(officerId, isSuperAdmin, letterId);
            if (letter.Status == LetterStatus.Cancelled)
                throw new AppException("This letter has been cancelled.");

            var pdf = LetterPdfBuilder.Build(letter);

            await letters.AddPrintLogAsync(new LetterPrintLog
            {
                LetterId = letterId,
                OfficerId = officerId,
                Action = action,
                IpAddress = ipAddress,
                ActionAt = DateTime.UtcNow
            });

            if (action == PrintActions.Print)
            {
                letter.PrintCount++;
                letter.FirstPrintedAt ??= DateTime.UtcNow;
                letter.Status = LetterStatus.Printed;
                await letters.SaveAsync();
            }

            return pdf;
        }

        private async Task<ForwardingLetter> GetAccessibleAsync(int officerId, bool isSuperAdmin, int letterId)
        {
            var letter = await letters.GetAsync(letterId);
            // Another officer's letter is reported as not found.
            if (letter is null || (!isSuperAdmin && letter.OfficerId != officerId))
                throw AppException.NotFound("Letter not found.");
            return letter;
        }

        private static LetterDto Map(ForwardingLetter l) => new()
        {
            LetterId = l.LetterId,
            ReferenceNumber = l.ReferenceNumber,
            LetterType = l.LetterType,
            Status = l.Status,
            CustomerName = l.CustomerName,
            AccountNumber = l.AccountNumber,
            CustomerAddress = l.CustomerAddress,
            CustomerMobile = l.CustomerMobile,
            CustomerEmail = l.CustomerEmail,
            BranchName = l.BranchName,
            ServiceName = l.ServiceName,
            PurposeName = l.PurposeName,
            TemplateId = l.TemplateId,
            Subject = l.SubjectSnapshot,
            Body = l.BodySnapshot,
            AddressedTo = l.AddressedTo,
            Enclosures = l.Enclosures,
            OfficerId = l.OfficerId,
            OfficerName = l.Officer.FullName,
            OfficerEmployeeId = l.Officer.EmployeeId,
            GeneratedAt = l.GeneratedAt,
            FirstPrintedAt = l.FirstPrintedAt,
            PrintCount = l.PrintCount
        };
    }
}
