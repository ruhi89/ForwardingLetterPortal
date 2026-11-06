namespace ForwardingLetterPortal.Interface.Constants
{
    public static class Roles
    {
        public const string Officer = "Officer";
        public const string SuperAdmin = "SuperAdmin";
        public static readonly string[] All = [Officer, SuperAdmin];
    }

    public static class LetterTypes
    {
        public const string CustomerCopy = "CustomerCopy";
        public const string InternalCopy = "InternalCopy";
        public static bool IsValid(string? value) => value is CustomerCopy or InternalCopy;
    }

    public static class OtpStatus
    {
        public const string Pending = "Pending";
        public const string Verified = "Verified";
        public const string Failed = "Failed";
        public const string Expired = "Expired";
        public const string NotMatched = "NotMatched";
    }

    public static class LetterStatus
    {
        public const string Generated = "Generated";
        public const string Printed = "Printed";
        public const string Cancelled = "Cancelled";
    }

    public static class PrintActions
    {
        public const string Preview = "Preview";
        public const string Print = "Print";
    }

    public static class VerificationRules
    {
        public const int MaxOtpAttempts = 5;
        public const int OtpLifetimeMinutes = 10;
        /// <summary>How long a verified OTP session may be used to generate letters.</summary>
        public const int VerifiedSessionMinutes = 30;
    }

    public static class LoginRules
    {
        public const int MaxFailedAttempts = 5;
        public const int LockoutMinutes = 15;
    }
}
