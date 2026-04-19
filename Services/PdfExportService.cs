using FreshEstimate.Mobile.Models;

#if ANDROID
using AndroidPrintAttributes = Android.Print.PrintAttributes;
using AndroidPrintedPdfDocument = Android.Print.Pdf.PrintedPdfDocument;
using AndroidCanvas = Android.Graphics.Canvas;
using AndroidPaint = Android.Graphics.Paint;
using AndroidColor = Android.Graphics.Color;
using AndroidPdfDocument = Android.Graphics.Pdf.PdfDocument;
#endif

namespace FreshEstimate.Mobile.Services;

public sealed class PdfExportService
{
    public byte[] GeneratePdf(BusinessDocument document, BrandingSettings branding)
    {
#if ANDROID
        return GenerateAndroidPdf(document, branding);
#else
        throw new PlatformNotSupportedException("This PDF generator is implemented for Android in this app.");
#endif
    }

#if ANDROID
    private static byte[] GenerateAndroidPdf(BusinessDocument document, BrandingSettings branding)
    {
        var attributes = new AndroidPrintAttributes.Builder()
            .SetMediaSize(AndroidPrintAttributes.MediaSize.IsoA4)
            .SetResolution(new AndroidPrintAttributes.Resolution("pdf", "pdf", 300, 300))
            .SetMinMargins(AndroidPrintAttributes.Margins.NoMargins)
            .Build();

        using var pdf = new AndroidPrintedPdfDocument(Android.App.Application.Context, attributes);

        const int pageWidth = 595;
        const int pageHeight = 842;
        const int margin = 36;
        const int lineHeight = 18;

        var paint = new AndroidPaint
        {
            AntiAlias = true,
            Color = AndroidColor.Black,
            TextSize = 11f
        };

        var boldPaint = new AndroidPaint(paint)
        {
            FakeBoldText = true
        };

        var titlePaint = new AndroidPaint(paint)
        {
            FakeBoldText = true,
            TextSize = 20f
        };

        var smallPaint = new AndroidPaint(paint)
        {
            TextSize = 9f
        };

        int pageNumber = 1;
        AndroidPdfDocument.Page? page = null;
        AndroidCanvas? canvas = null;
        int y = margin;

        void StartNewPage()
        {
            page?.Dispose();

            var pageInfo = new AndroidPdfDocument.PageInfo.Builder(pageWidth, pageHeight, pageNumber).Create();
            page = pdf.StartPage(pageInfo);
            canvas = page.Canvas;
            y = margin;
            pageNumber++;
        }

        void FinishCurrentPage()
        {
            if (page != null)
            {
                pdf.FinishPage(page);
                page = null;
                canvas = null;
            }
        }

        void EnsureSpace(int neededHeight)
        {
            if (canvas == null)
                StartNewPage();

            if (y + neededHeight > pageHeight - margin)
            {
                FinishCurrentPage();
                StartNewPage();
            }
        }

        void DrawText(string text, AndroidPaint p, int x)
        {
            EnsureSpace(lineHeight);
            canvas!.DrawText(text ?? string.Empty, x, y, p);
            y += lineHeight;
        }

        void DrawDivider()
        {
            EnsureSpace(12);
            canvas!.DrawLine(margin, y, pageWidth - margin, y, paint);
            y += 12;
        }

        void DrawKeyValue(string left, string right)
        {
            EnsureSpace(lineHeight);
            canvas!.DrawText(left ?? string.Empty, margin, y, paint);
            var rightWidth = paint.MeasureText(right ?? string.Empty);
            canvas.DrawText(right ?? string.Empty, pageWidth - margin - rightWidth, y, paint);
            y += lineHeight;
        }

        StartNewPage();

        DrawText(branding.BusinessName ?? "Fresh Estimate and Invoicing", titlePaint, margin);
        if (!string.IsNullOrWhiteSpace(branding.BusinessEmail))
            DrawText(branding.BusinessEmail, paint, margin);
        if (!string.IsNullOrWhiteSpace(branding.BusinessPhone))
            DrawText(branding.BusinessPhone, paint, margin);
        if (!string.IsNullOrWhiteSpace(branding.AddressLine1))
            DrawText(branding.AddressLine1, paint, margin);
        if (!string.IsNullOrWhiteSpace(branding.AddressLine2))
            DrawText(branding.AddressLine2, paint, margin);

        y += 6;
        DrawDivider();

        DrawText($"{document.TypeDisplay.ToUpperInvariant()} #{document.Number}", titlePaint, margin);
        DrawText($"Issue Date: {document.IssueDate:d}", paint, margin);

        if (document.Type == DocumentType.Invoice)
            DrawText($"Due Date: {document.DueDate:d}", paint, margin);
        else
            DrawText($"Valid Until: {document.ValidUntil:d}", paint, margin);

        DrawText($"Status: {document.Status}", boldPaint, margin);

        y += 6;
        DrawDivider();

        DrawText("Bill To", boldPaint, margin);
        DrawText(document.Customer?.DisplayName ?? document.CustomerDisplayName ?? string.Empty, paint, margin);
        if (!string.IsNullOrWhiteSpace(document.Customer?.ContactName))
            DrawText(document.Customer.ContactName, paint, margin);
        if (!string.IsNullOrWhiteSpace(document.Customer?.Email))
            DrawText(document.Customer.Email, paint, margin);
        if (!string.IsNullOrWhiteSpace(document.Customer?.Phone))
            DrawText(document.Customer.Phone, paint, margin);
        if (!string.IsNullOrWhiteSpace(document.Customer?.BillingAddress))
            DrawText(document.Customer.BillingAddress, paint, margin);

        y += 6;
        DrawDivider();

        DrawText("Summary", boldPaint, margin);
        DrawText(document.Title ?? string.Empty, paint, margin);
        DrawText(document.DateDisplay ?? string.Empty, paint, margin);

        y += 6;
        DrawDivider();

        DrawText("Line Items", boldPaint, margin);
        DrawKeyValue("Description", "Total");

        foreach (var item in document.Items)
        {
            EnsureSpace(lineHeight * 3);
            DrawText(item.Description ?? string.Empty, paint, margin);
            DrawKeyValue(
                $"Qty {item.Quantity:0.##}  ×  {item.UnitPrice:C2}  Tax {item.TaxRatePercent:0.##}%",
                item.Total.ToString("C2"));
        }

        y += 6;
        DrawDivider();

        DrawKeyValue("Subtotal", document.Subtotal.ToString("C2"));
        DrawKeyValue("Tax", document.TaxTotal.ToString("C2"));
        DrawKeyValue("Total", document.GrandTotal.ToString("C2"));

        if (!string.IsNullOrWhiteSpace(document.Notes))
        {
            y += 6;
            DrawDivider();
            DrawText("Notes", boldPaint, margin);
            foreach (var line in SplitLines(document.Notes, 80))
                DrawText(line, paint, margin);
        }

        if (!string.IsNullOrWhiteSpace(branding.PaymentInstructions))
        {
            y += 6;
            DrawDivider();
            DrawText("Payment / Terms", boldPaint, margin);
            foreach (var line in SplitLines(branding.PaymentInstructions, 80))
                DrawText(line, paint, margin);
        }

        y += 12;
        DrawDivider();
        DrawText("Generated by Fresh Estimate and Invoicing", smallPaint, margin);

        FinishCurrentPage();

        using var stream = new MemoryStream();
        pdf.WriteTo(stream);
        return stream.ToArray();
    }

    private static IEnumerable<string> SplitLines(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var paragraphs = text.Replace("\r\n", "\n").Split('\n');

        foreach (var paragraph in paragraphs)
        {
            var remaining = paragraph.Trim();

            if (string.IsNullOrEmpty(remaining))
            {
                yield return string.Empty;
                continue;
            }

            while (remaining.Length > maxLength)
            {
                int breakAt = remaining.LastIndexOf(' ', Math.Min(maxLength, remaining.Length - 1));
                if (breakAt <= 0)
                    breakAt = Math.Min(maxLength, remaining.Length);

                yield return remaining[..breakAt].TrimEnd();
                remaining = remaining[breakAt..].TrimStart();
            }

            if (!string.IsNullOrEmpty(remaining))
                yield return remaining;
        }
    }
#endif
}