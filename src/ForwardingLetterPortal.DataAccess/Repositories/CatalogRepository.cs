using ForwardingLetterPortal.Interface.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForwardingLetterPortal.DataAccess.Repositories
{
    public class CatalogRepository(AppDbContext db)
    {
        // ---- services & purposes ----
        public Task<List<RetailService>> GetActiveServicesAsync()
            => db.RetailServices.AsNoTracking().Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder).ToListAsync();

        public Task<RetailService?> GetServiceAsync(int serviceId)
            => db.RetailServices.AsNoTracking().FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        public Task<List<ServicePurpose>> GetActivePurposesAsync(int serviceId)
            => db.ServicePurposes.AsNoTracking().Where(p => p.ServiceId == serviceId && p.IsActive)
                .OrderBy(p => p.DisplayOrder).ToListAsync();

        public Task<ServicePurpose?> GetPurposeAsync(int purposeId)
            => db.ServicePurposes.AsNoTracking().FirstOrDefaultAsync(p => p.PurposeId == purposeId);

        // ---- templates ----
        public Task<List<LetterTemplate>> ListTemplatesAsync(int? serviceId, int? purposeId, string? letterType, bool? isActive)
            => db.LetterTemplates.AsNoTracking()
                .Include(t => t.Service).Include(t => t.Purpose)
                .Where(t => (serviceId == null || t.ServiceId == serviceId)
                         && (purposeId == null || t.PurposeId == purposeId)
                         && (letterType == null || t.LetterType == letterType)
                         && (isActive == null || t.IsActive == isActive))
                .OrderBy(t => t.Service.DisplayOrder).ThenBy(t => t.Purpose.DisplayOrder)
                .ThenBy(t => t.LetterType).ThenByDescending(t => t.IsActive).ThenByDescending(t => t.TemplateId)
                .ToListAsync();

        public Task<LetterTemplate?> GetTemplateAsync(int templateId)
            => db.LetterTemplates.Include(t => t.Service).Include(t => t.Purpose)
                .FirstOrDefaultAsync(t => t.TemplateId == templateId);

        public Task<LetterTemplate?> FindActiveTemplateAsync(int serviceId, int purposeId, string letterType)
            => db.LetterTemplates.AsNoTracking().FirstOrDefaultAsync(t =>
                t.ServiceId == serviceId && t.PurposeId == purposeId && t.LetterType == letterType && t.IsActive);

        /// <summary>
        /// Inserts or saves a template. When it is active, any other active template for the same
        /// service/purpose/letter type is deactivated first, in one transaction, so the filtered
        /// unique index (one active template per key) is never violated.
        /// </summary>
        public async Task SaveTemplateAsync(LetterTemplate template, int? actingOfficerId)
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            if (template.IsActive)
            {
                var now = DateTime.UtcNow;
                await db.LetterTemplates
                    .Where(t => t.IsActive && t.TemplateId != template.TemplateId
                             && t.ServiceId == template.ServiceId
                             && t.PurposeId == template.PurposeId
                             && t.LetterType == template.LetterType)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(t => t.IsActive, false)
                        .SetProperty(t => t.UpdatedAt, now)
                        .SetProperty(t => t.UpdatedByOfficerId, actingOfficerId));
            }

            if (template.TemplateId == 0)
                db.LetterTemplates.Add(template);

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
    }
}
