using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.DTOs.Catalog;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;

namespace ForwardingLetterPortal.Service
{
    public class CatalogService(CatalogRepository repo) : ICatalogService
    {
        public async Task<List<ServiceDto>> GetServicesAsync()
            => (await repo.GetActiveServicesAsync()).Select(s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                ServiceCode = s.ServiceCode,
                ServiceName = s.ServiceName
            }).ToList();

        public async Task<List<PurposeDto>> GetPurposesAsync(int serviceId)
        {
            var service = await repo.GetServiceAsync(serviceId);
            if (service is null || !service.IsActive)
                throw AppException.NotFound("Service not found.");

            return (await repo.GetActivePurposesAsync(serviceId)).Select(p => new PurposeDto
            {
                PurposeId = p.PurposeId,
                ServiceId = p.ServiceId,
                PurposeName = p.PurposeName
            }).ToList();
        }
    }
}
