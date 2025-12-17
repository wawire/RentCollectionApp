using System.Text.RegularExpressions;

namespace RentCollection.Application.Services
{
    /// <summary>
    /// Service for rendering message templates with variable substitution
    /// </summary>
    public class MessageTemplateService
    {
        private static readonly Dictionary<string, string> DefaultTemplates = new()
        {
            ["7DaysBefore"] = "Hi {tenantName}, this is a friendly reminder that your rent of KSh {amount} is due on {dueDate}. Property: {propertyName}, Unit: {unitNumber}. Pay via M-Pesa to avoid late fees. - {landlordName}",
            ["3DaysBefore"] = "Hi {tenantName}, your rent of KSh {amount} is due in 3 days ({dueDate}). Please ensure payment is made on time to avoid late fees. Unit: {unitNumber}. - {landlordName}",
            ["1DayBefore"] = "Hi {tenantName}, reminder: Your rent of KSh {amount} is due tomorrow ({dueDate}). Unit: {unitNumber}. Pay now to avoid late fees. - {landlordName}",
            ["OnDueDate"] = "Hi {tenantName}, your rent of KSh {amount} is due TODAY ({dueDate}). Please pay as soon as possible. Unit: {unitNumber}. - {landlordName}",
            ["1DayOverdue"] = "Hi {tenantName}, your rent of KSh {amount} was due yesterday ({dueDate}) and is now OVERDUE. Late fees may apply. Please pay immediately. Unit: {unitNumber}. - {landlordName}",
            ["3DaysOverdue"] = "Hi {tenantName}, your rent of KSh {amount} is now 3 days overdue (due: {dueDate}). Late fees are being applied. Please pay immediately to avoid further action. Unit: {unitNumber}. - {landlordName}",
            ["7DaysOverdue"] = "URGENT: Hi {tenantName}, your rent of KSh {amount} is now 7 days overdue (due: {dueDate}). Immediate payment required. Contact {landlordPhone} to discuss. Unit: {unitNumber}. - {landlordName}"
        };

        /// <summary>
        /// Get default template for a reminder type
        /// </summary>
        public string GetDefaultTemplate(string reminderType)
        {
            return DefaultTemplates.TryGetValue(reminderType, out var template)
                ? template
                : DefaultTemplates["7DaysBefore"];
        }

        /// <summary>
        /// Render a template by replacing variables with actual values
        /// </summary>
        public string RenderTemplate(string template, Dictionary<string, string> variables)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            var result = template;

            // Replace each variable in the template
            foreach (var kvp in variables)
            {
                var placeholder = $"{{{kvp.Key}}}";
                result = result.Replace(placeholder, kvp.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }

            // Remove any unreplaced variables (in case template has variables we don't have data for)
            result = Regex.Replace(result, @"\{[^}]+\}", string.Empty);

            return result.Trim();
        }

        /// <summary>
        /// Build variables dictionary from tenant/payment data
        /// </summary>
        public Dictionary<string, string> BuildVariables(
            string tenantName,
            string tenantPhone,
            string landlordName,
            string landlordPhone,
            string propertyName,
            string unitNumber,
            decimal rentAmount,
            DateTime dueDate,
            int? daysUntilDue = null)
        {
            var formattedDueDate = dueDate.ToString("dddd, dd MMMM yyyy"); // e.g., "Monday, 05 January 2026"
            var formattedAmount = rentAmount.ToString("N0"); // e.g., "15,000"

            var variables = new Dictionary<string, string>
            {
                ["tenantName"] = tenantName,
                ["tenantFirstName"] = tenantName.Split(' ').FirstOrDefault() ?? tenantName,
                ["tenantPhone"] = tenantPhone,
                ["landlordName"] = landlordName,
                ["landlordPhone"] = landlordPhone,
                ["propertyName"] = propertyName,
                ["unitNumber"] = unitNumber,
                ["amount"] = formattedAmount,
                ["rentAmount"] = formattedAmount,
                ["dueDate"] = formattedDueDate,
                ["dueDateShort"] = dueDate.ToString("dd/MM/yyyy"),
                ["today"] = DateTime.Today.ToString("dd/MM/yyyy"),
                ["year"] = DateTime.Today.Year.ToString()
            };

            if (daysUntilDue.HasValue)
            {
                variables["daysUntilDue"] = Math.Abs(daysUntilDue.Value).ToString();
                variables["daysOverdue"] = daysUntilDue.Value < 0 ? Math.Abs(daysUntilDue.Value).ToString() : "0";
            }

            return variables;
        }

        /// <summary>
        /// Validate template syntax (check for balanced braces)
        /// </summary>
        public bool IsValidTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return false;
            }

            var openBraces = template.Count(c => c == '{');
            var closeBraces = template.Count(c => c == '}');

            return openBraces == closeBraces;
        }

        /// <summary>
        /// Extract variable names from template
        /// </summary>
        public List<string> ExtractVariables(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return new List<string>();
            }

            var matches = Regex.Matches(template, @"\{([^}]+)\}");
            return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
        }
    }
}
