using CommunityToolkit.Maui.Storage;
using FreshEstimate.Mobile.Models;
using System.Text;

namespace FreshEstimate.Mobile.Services;

public sealed class CsvCustomerService
{
    public async Task ExportAsync(IEnumerable<Customer> customers, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("DisplayName,ContactName,Email,Phone,BillingAddress,Notes");

        foreach (var customer in customers.OrderBy(x => x.DisplayName))
        {
            sb.AppendLine(string.Join(",",
                CsvEscape(customer.DisplayName),
                CsvEscape(customer.ContactName),
                CsvEscape(customer.Email),
                CsvEscape(customer.Phone),
                CsvEscape(customer.BillingAddress),
                CsvEscape(customer.Notes)));
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        var result = await FileSaver.Default.SaveAsync("customers.csv", stream, cancellationToken);

        if (!result.IsSuccessful)
            throw result.Exception ?? new InvalidOperationException("CSV export failed.");
    }

    public async Task<List<Customer>?> ImportAsync(CancellationToken cancellationToken = default)
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select customer CSV"
        });

        if (file is null)
            return null;

        await using var source = await file.OpenReadAsync();
        using var reader = new StreamReader(source);
        var text = await reader.ReadToEndAsync(cancellationToken);

        var lines = text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            return new List<Customer>();

        var results = new List<Customer>();

        for (int i = 1; i < lines.Length; i++)
        {
            var parts = ParseCsvLine(lines[i]);
            while (parts.Count < 6)
                parts.Add(string.Empty);

            if (string.IsNullOrWhiteSpace(parts[0]))
                continue;

            results.Add(new Customer
            {
                DisplayName = parts[0],
                ContactName = parts[1],
                Email = parts[2],
                Phone = parts[3],
                BillingAddress = parts[4],
                Notes = parts[5]
            });
        }

        return results;
    }

    private static string CsvEscape(string? value)
    {
        value ??= string.Empty;
        value = value.Replace("\"", "\"\"");

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value}\"";

        return value;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
        }

        result.Add(sb.ToString());
        return result;
    }
}
