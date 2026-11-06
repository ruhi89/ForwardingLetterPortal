using ForwardingLetterPortal.Interface.DTOs.Templates;
using System.Text.RegularExpressions;

namespace ForwardingLetterPortal.Service
{
    /// <summary>Renders {{Placeholder}} tokens in template subject/body text.</summary>
    public static partial class TemplateRenderer
    {
        public static readonly IReadOnlyDictionary<string, string> Placeholders =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CustomerName"] = "Customer full name",
                ["FatherName"] = "Customer father's name",
                ["AccountNumber"] = "Customer account number",
                ["CustomerAddress"] = "Customer address",
                ["CustomerMobile"] = "Customer registered mobile number",
                ["CustomerEmail"] = "Customer registered e-mail",
                ["BranchName"] = "Branch where the account is held",
                ["ServiceName"] = "Selected retail service",
                ["PurposeName"] = "Selected purpose",
                ["ReferenceNumber"] = "Letter reference number",
                ["Date"] = "Letter date (dd MMMM yyyy)",
                ["OfficerName"] = "Issuing officer name",
                ["OfficerDesignation"] = "Issuing officer designation",
                ["OfficerBranch"] = "Issuing officer branch"
            };

        [GeneratedRegex(@"\{\{\s*(\w+)\s*\}\}")]
        private static partial Regex PlaceholderPattern();

        public static List<PlaceholderDto> Describe()
            => Placeholders.Select(p => new PlaceholderDto { Name = p.Key, Description = p.Value }).ToList();

        public static List<string> FindUnknown(string text)
            => PlaceholderPattern().Matches(text)
                .Select(m => m.Groups[1].Value)
                .Where(name => !Placeholders.ContainsKey(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        public static string Render(string text, IReadOnlyDictionary<string, string> values)
            => PlaceholderPattern().Replace(text, m =>
                values.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);
    }
}
