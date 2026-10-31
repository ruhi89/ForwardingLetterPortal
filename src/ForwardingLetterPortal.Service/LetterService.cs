using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.DTOs.Letter;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ForwardingLetterPortal.Service
{
    public class LetterService(LetterRepository repo) : ILetterService
    {
        public async Task<LetterRequestResponseDto> CreateRequestAsync(int customerId, CreateLetterRequestDto dto)
        {
            var request = new ForwardingLetterRequest
            {
                CustomerId = customerId,
                RecipientName = dto.RecipientName,
                RecipientBankName = dto.RecipientBankName,
                RecipientBranchName = dto.RecipientBranchName,
                RecipientAddress = dto.RecipientAddress,
                Purpose = dto.Purpose,
                DocumentDescription = dto.DocumentDescription,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };

            await repo.AddRequestAsync(request);

            return MapToDto(request);
        }

        public async Task<List<LetterRequestResponseDto>> GetMyRequestsAsync(int customerId)
        {
            var requests = await repo.GetByCustomerAsync(customerId);
            return requests.Select(MapToDto).ToList();
        }

        public async Task<byte[]> GeneratePdfAsync(int requestId, int customerId)
        {
            var request = await repo.GetByIdAsync(requestId, customerId)
                ?? throw new Exception("Request not found.");

            var pdfBytes = BuildLetterPdf(request);

            var letter = new GeneratedLetter
            {
                RequestId = requestId,
                CustomerId = customerId,
                FilePath = $"letters/{requestId}.pdf",
                FileSize = pdfBytes.Length,
                GeneratedAt = DateTime.UtcNow
            };

            if (request.GeneratedLetter is null)
                await repo.SaveLetterAsync(letter);

            await repo.UpdateRequestStatusAsync(requestId, "Generated");

            return pdfBytes;
        }

        private static byte[] BuildLetterPdf(ForwardingLetterRequest r)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var ba = r.Customer.BankAccount;

            return Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(t => t.FontSize(11));

                page.Content().Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Text("Forwarding Letter").FontSize(18).Bold();
                    col.Item().Text($"Date: {DateTime.UtcNow:dd MMM yyyy}");

                    col.Item().Column(to =>
                    {
                        to.Item().Text("To,");
                        to.Item().Text(r.RecipientName);
                        to.Item().Text(r.RecipientBankName);
                        to.Item().Text(r.RecipientBranchName);
                        to.Item().Text(r.RecipientAddress);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Subject: ").Bold();
                        t.Span(r.Purpose);
                    });

                    col.Item().Text("Dear Sir/Madam,");
                    col.Item().Text(t =>
                    {
                        t.Span("We are forwarding the following documents on behalf of ");
                        t.Span(ba.FullName).Bold();
                        t.Span($" (Account No: {ba.AccountNumber}):");
                    });
                    col.Item().Text(r.DocumentDescription);
                    col.Item().Text("Please acknowledge receipt.");

                    col.Item().PaddingTop(20).Column(sig =>
                    {
                        sig.Item().Text("Sincerely,");
                        sig.Item().Text(ba.BranchName);
                    });
                });
            })).GeneratePdf();
        }

        private static LetterRequestResponseDto MapToDto(ForwardingLetterRequest r) => new()
        {
            RequestId = r.RequestId,
            RecipientName = r.RecipientName,
            RecipientBankName = r.RecipientBankName,
            Purpose = r.Purpose,
            Status = r.Status,
            SubmittedAt = r.SubmittedAt,
            HasGeneratedLetter = r.GeneratedLetter is not null
        };
    }
}
