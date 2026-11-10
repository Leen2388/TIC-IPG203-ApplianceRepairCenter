using System.Text.RegularExpressions;

namespace ApplianceRepairCenter.Utilities;

public static class InputValidator
{
    public static string RequiredText(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} is required.");
        return value.Trim();
    }

    public static string SerialNumber(string value)
    {
        string serial = RequiredText(value, "Serial number").ToUpperInvariant();
        if (!Regex.IsMatch(serial, @"^[A-Z0-9][A-Z0-9-]{2,29}$"))
            throw new ArgumentException("Serial must be 3-30 letters, digits or hyphens.");
        return serial;
    }

    public static decimal NonNegative(decimal value, string field)
    {
        if (value < 0 || value > 1_000_000)
            throw new ArgumentOutOfRangeException(field, "Cost must be between 0 and 1000000.");
        return value;
    }

    public static void Dates(DateTime received, DateTime expected)
    {
        if (received.Date > DateTime.Today)
            throw new ArgumentException("Received date cannot be in the future.");
        if (expected.Date < received.Date)
            throw new ArgumentException("Expected date cannot precede received date.");
    }
}
