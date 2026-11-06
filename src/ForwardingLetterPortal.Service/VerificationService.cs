using ForwardingLetterPortal.DataAccess.Repositories;
using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.DTOs.Verification;
using ForwardingLetterPortal.Interface.Entities;
using ForwardingLetterPortal.Interface.Exceptions;
using ForwardingLetterPortal.Interface.Interfaces;
using System.Security.Cryptography;

namespace ForwardingLetterPortal.Service
{
    public class VerificationService(VerificationRepository repo, IEmailService email) : IVerificationService
    {
        private const string ChannelEmail = "Email";

        public async Task<StartVerificationResponseDto> StartAsync(int officerId, StartVerificationDto dto, string? ipAddress)
        {
            var accountNumber = dto.AccountNumber.Trim();
            var contact = dto.Email.Trim();
            const string channel = ChannelEmail;

            var account = await repo.FindAccountAsync(accountNumber);

            // Steps 5-6: check the entered details against bank records.
            string? failure = null;
            string? failureMessage = null;
            if (account is null)
            {
                failure = "AccountNotFound";
                failureMessage = "Account not found.";
            }
            else if (!string.Equals(account.AccountStatus, "Active", StringComparison.OrdinalIgnoreCase))
            {
                failure = "AccountNotActive";
                failureMessage = $"Account is {account.AccountStatus}. A letter cannot be issued.";
            }
            else if (string.IsNullOrWhiteSpace(account.Email))
            {
                failure = "NoEmailOnFile";
                failureMessage = "No e-mail address is registered for this account.";
            }
            else if (!string.Equals(account.Email.Trim(), contact, StringComparison.OrdinalIgnoreCase))
            {
                failure = "ContactMismatch";
                failureMessage = "The e-mail does not match the bank's records for this account.";
            }

            var verification = new OtpVerification
            {
                OfficerId = officerId,
                AccountNumber = accountNumber,
                BankAccountId = account?.BankAccountId,
                ContactEntered = contact,
                Channel = channel,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            if (failure is not null)
            {
                verification.Status = OtpStatus.NotMatched;
                verification.FailureReason = failure;
                await repo.AddAsync(verification);
                return new StartVerificationResponseDto { Success = false, Message = failureMessage! };
            }

            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            verification.OtpHash = BCrypt.Net.BCrypt.HashPassword(otp);
            verification.OtpExpiresAt = DateTime.UtcNow.AddMinutes(VerificationRules.OtpLifetimeMinutes);
            verification.Status = OtpStatus.Pending;
            await repo.AddAsync(verification);

            // The OTP always goes to the e-mail on file, never to what was typed.
            try
            {
                await email.SendAsync(account!.Email!, "Your verification code",
                    $"<p>Your Agrani Bank forwarding letter verification code is <b>{otp}</b>.</p>" +
                    $"<p>It expires in {VerificationRules.OtpLifetimeMinutes} minutes. Share it only with the bank officer assisting you.</p>");
            }
            catch
            {
                verification.Status = OtpStatus.Failed;
                verification.FailureReason = "SendFailed";
                await repo.SaveAsync();
                throw new AppException("Could not send the verification code. Please try again.", 502);
            }

            return new StartVerificationResponseDto
            {
                Success = true,
                VerificationId = verification.VerificationId,
                Channel = channel,
                MaskedContact = MaskEmail(account.Email!),
                OtpExpiresAt = verification.OtpExpiresAt,
                Message = "A verification code has been sent to the customer's registered e-mail."
            };
        }

        public async Task<ConfirmOtpResponseDto> ConfirmAsync(int officerId, int verificationId, ConfirmOtpDto dto)
        {
            var v = await GetOwnedAsync(officerId, verificationId);

            if (v.Status != OtpStatus.Pending)
                return Fail(v.Status == OtpStatus.Verified
                    ? "This verification is already complete."
                    : "This verification can no longer be used. Please start a new one.");

            if (v.OtpExpiresAt is null || v.OtpExpiresAt < DateTime.UtcNow)
            {
                v.Status = OtpStatus.Expired;
                v.FailureReason = "Expired";
                await repo.SaveAsync();
                return Fail("The code has expired. Please start a new verification.");
            }

            v.AttemptCount++;

            if (!BCrypt.Net.BCrypt.Verify(dto.Otp, v.OtpHash))
            {
                var left = VerificationRules.MaxOtpAttempts - v.AttemptCount;
                v.FailureReason = "WrongOtp";
                if (left <= 0)
                    v.Status = OtpStatus.Failed;
                await repo.SaveAsync();
                return Fail(left > 0
                    ? $"Invalid code. {left} attempt(s) left."
                    : "Too many invalid attempts. Please start a new verification.");
            }

            v.Status = OtpStatus.Verified;
            v.FailureReason = null;
            v.VerifiedAt = DateTime.UtcNow;
            await repo.SaveAsync();

            return new ConfirmOtpResponseDto
            {
                Success = true,
                Message = "Customer verified.",
                Customer = MapCustomer(v.BankAccount!)
            };
        }

        public async Task<CustomerInfoDto> GetVerifiedCustomerAsync(int officerId, int verificationId)
        {
            var v = await GetOwnedAsync(officerId, verificationId);
            if (!IsUsable(v))
                throw new AppException("Customer verification is missing or has expired. Please verify again.");
            return MapCustomer(v.BankAccount!);
        }

        /// <summary>A verification can be used to read data / generate letters only while this holds.</summary>
        public static bool IsUsable(OtpVerification v)
            => v.Status == OtpStatus.Verified
               && v.BankAccount is not null
               && v.VerifiedAt is { } at
               && at.AddMinutes(VerificationRules.VerifiedSessionMinutes) > DateTime.UtcNow;

        private async Task<OtpVerification> GetOwnedAsync(int officerId, int verificationId)
        {
            var v = await repo.GetAsync(verificationId);
            // Another officer's session is reported as not found.
            if (v is null || v.OfficerId != officerId)
                throw AppException.NotFound("Verification not found.");
            return v;
        }

        private static ConfirmOtpResponseDto Fail(string message) => new() { Success = false, Message = message };

        private static string MaskEmail(string e)
        {
            var at = e.IndexOf('@');
            return at <= 1 ? e : e[0] + new string('*', at - 1) + e[at..];
        }

        private static CustomerInfoDto MapCustomer(BankAccount a) => new()
        {
            AccountNumber = a.AccountNumber,
            FullName = a.FullName,
            FatherName = a.FatherName,
            Address = a.Address,
            Email = a.Email,
            MobileNumber = a.MobileNumber,
            BranchName = a.BranchName,
            ServiceName = a.Service?.ServiceName,
            OpenedOn = a.OpenedOn,
            AccountStatus = a.AccountStatus
        };
    }
}
