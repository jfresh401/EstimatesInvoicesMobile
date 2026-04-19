using FreshEstimate.Mobile.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FreshEstimate.Mobile.Services;

public sealed class PdfExportService
{
    private readonly EmbeddedAppLogoService _logoService;
    private static bool _questPdfInitialized;
    private static readonly object _questPdfInitLock = new();

    public PdfExportService(EmbeddedAppLogoService logoService)
    {
        _logoService = logoService;
    }

    private static void EnsureQuestPdfInitialized()
    {
        if (_questPdfInitialized)
            return;

        lock (_questPdfInitLock)
        {
            if (_questPdfInitialized)
                return;

            QuestPDF.Settings.License = LicenseType.Community;
            _questPdfInitialized = true;
        }
    }

    public byte[] GeneratePdf(BusinessDocument document, BrandingSettings branding)
    {
        EnsureQuestPdfInitialized();

        var appLogoBytes = _logoService.LoadAppLogoBytes();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Row(left =>
                        {
                            if (!string.IsNullOrWhiteSpace(branding.LogoFilePath) && File.Exists(branding.LogoFilePath))
                            {
                                left.ConstantItem(92).Height(92).AlignMiddle().Element(c =>
                                {
                                    c.Image(branding.LogoFilePath!).FitArea();
                                });
                                left.ConstantItem(12);
                            }

                            left.RelativeItem().Column(c =>
                            {
                                c.Item().Text(branding.BusinessName ?? string.Empty).FontSize(24).Bold();
                                c.Item().Text(branding.BusinessEmail ?? string.Empty);
                                c.Item().Text(branding.BusinessPhone ?? string.Empty);
                                c.Item().Text(branding.AddressLine1 ?? string.Empty);
                                c.Item().Text(branding.AddressLine2 ?? string.Empty);
                            });
                        });

                        row.ConstantItem(190).AlignRight().Column(right =>
                        {
                            right.Item().Text(document.TypeDisplay.ToUpperInvariant())
                                .FontSize(22)
                                .Bold()
                                .FontColor(branding.AccentHex);

                            right.Item().Text($"{document.TypeDisplay} #: {document.Number}");
                            right.Item().Text($"Issue Date: {document.IssueDate:d}");

                            if (document.Type == DocumentType.Invoice)
                                right.Item().Text($"Due Date: {document.DueDate:d}");
                            else
                                right.Item().Text($"Valid Until: {document.ValidUntil:d}");

                            right.Item().PaddingTop(5).Text($"Status: {document.Status}").SemiBold();
                        });
                    });

                    col.Item().PaddingTop(16).LineHorizontal(1).LineColor("#D8DEE6");
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Bill To").Bold().FontColor(branding.AccentHex);
                            c.Item().Text(document.Customer?.DisplayName ?? document.CustomerDisplayName ?? string.Empty);
                            c.Item().Text(document.Customer?.ContactName ?? string.Empty);
                            c.Item().Text(document.Customer?.Email ?? string.Empty);
                            c.Item().Text(document.Customer?.Phone ?? string.Empty);
                            c.Item().Text(document.Customer?.BillingAddress ?? string.Empty);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Summary").Bold().FontColor(branding.AccentHex);
                            c.Item().Text(document.Title ?? string.Empty);
                            c.Item().Text(document.DateDisplay ?? string.Empty);
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(1.1f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            static QuestPDF.Infrastructure.IContainer Cell(QuestPDF.Infrastructure.IContainer c) =>
                                c.PaddingVertical(8).PaddingHorizontal(6).Background("#F4F7FB");

                            header.Cell().Element(Cell).Text("Description").Bold();
                            header.Cell().Element(Cell).AlignRight().Text("Qty").Bold();
                            header.Cell().Element(Cell).AlignRight().Text("Unit").Bold();
                            header.Cell().Element(Cell).AlignRight().Text("Tax %").Bold();
                            header.Cell().Element(Cell).AlignRight().Text("Total").Bold();
                        });

                        foreach (var item in document.Items)
                        {
                            static QuestPDF.Infrastructure.IContainer BodyCell(QuestPDF.Infrastructure.IContainer c) =>
                                c.PaddingVertical(8).PaddingHorizontal(6).BorderBottom(1).BorderColor("#EDF1F5");

                            table.Cell().Element(BodyCell).Text(item.Description ?? string.Empty);
                            table.Cell().Element(BodyCell).AlignRight().Text(item.Quantity.ToString("0.##"));
                            table.Cell().Element(BodyCell).AlignRight().Text(item.UnitPrice.ToString("C2"));
                            table.Cell().Element(BodyCell).AlignRight().Text(item.TaxRatePercent.ToString("0.##"));
                            table.Cell().Element(BodyCell).AlignRight().Text(item.Total.ToString("C2"));
                        }
                    });

                    col.Item().AlignRight().Width(240).Column(totals =>
                    {
                        totals.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal");
                            row.ConstantItem(100).AlignRight().Text(document.Subtotal.ToString("C2"));
                        });

                        totals.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Tax");
                            row.ConstantItem(100).AlignRight().Text(document.TaxTotal.ToString("C2"));
                        });

                        totals.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text("Total").Bold();
                            row.ConstantItem(100).AlignRight().Text(document.GrandTotal.ToString("C2")).Bold().FontSize(14);
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(document.Notes))
                    {
                        col.Item().PaddingTop(6).Column(notes =>
                        {
                            notes.Item().Text("Notes").Bold().FontColor(branding.AccentHex);
                            notes.Item().Border(1).BorderColor("#D8DEE6").Padding(10).Text(document.Notes);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(branding.PaymentInstructions))
                    {
                        col.Item().PaddingTop(4).Column(terms =>
                        {
                            terms.Item().Text("Payment / Terms").Bold().FontColor(branding.AccentHex);
                            terms.Item().Border(1).BorderColor("#D8DEE6").Padding(10).Text(branding.PaymentInstructions);
                        });
                    }
                });

                page.Footer().PaddingTop(10).Row(row =>
                {
                    if (appLogoBytes != null)
                    {
                        row.ConstantItem(34)
                            .Height(34)
                            .Element(c => c.Image(appLogoBytes).FitArea());
                    }
                    else
                    {
                        row.ConstantItem(34);
                    }

                    row.ConstantItem(8);

                    row.RelativeItem().AlignMiddle().Column(col =>
                    {
                        col.Item().Text("Generated by")
                            .FontSize(8)
                            .FontColor(QuestPDF.Helpers.Colors.Grey.Lighten1);

                        col.Item().Text("Fresh Estimates and Invoicing")
                            .FontSize(10)
                            .SemiBold();
                    });
                });
            });
        }).GeneratePdf();
    }
}