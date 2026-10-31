using ForwardingLetterPortal.Interface.DTOs.Letter;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Interface.Interfaces
{
    public interface ILetterService
    {
        Task<LetterRequestResponseDto> CreateRequestAsync(int customerId, CreateLetterRequestDto dto);
        Task<List<LetterRequestResponseDto>> GetMyRequestsAsync(int customerId);
        Task<byte[]> GeneratePdfAsync(int requestId, int customerId);
    }
}
