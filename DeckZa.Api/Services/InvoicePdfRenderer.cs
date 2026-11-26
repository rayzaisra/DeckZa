using System.Globalization;

using QuestPDF.Fluent;
using QuestPDF.Helpers;

using DeckZa.Application.DTOs.Payments;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Services;

/// <summary>
/// Merender invoice pembayaran menjadi PDF A4 portrait.
/// Implementasi ini memakai QuestPDF, yang hanya direference di project Api —
/// karena itu kontrak <see cref="IInvoiceRenderer"/> berada di Application.
/// </summary>
public class InvoicePdfRenderer : IInvoiceRenderer
{
    /// <summary>Nama aplikasi pada kop invoice. Satu konstanta, tidak diulang.</summary>
    public const string ApplicationName = "DeckZa";

    private const string ApplicationTagline = "Personal Task and Report Management";

    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    public byte[] Render(InvoiceRenderModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(32);
                page.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Black));

                page.Header().Column(header =>
                {
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(ApplicationName).FontSize(18).SemiBold();
                            column.Item().Text(ApplicationTagline).FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(120).AlignRight().Text("INVOICE")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor(Colors.Black);
                    });

                    header.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);
                    header.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Invoice No. ").SemiBold();
                            text.Span(model.InvoiceNo);
                        });

                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Date. ").SemiBold();
                            text.Span(model.InvoiceDate.ToString("dd MMM yyyy", IndonesianCulture));
                        });

                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Status. ").SemiBold();
                            text.Span("PAID");
                        });
                    });
                });

                page.Content().PaddingVertical(14).Column(content =>
                {
                    content.Item().Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("BILLED TO").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(4).Text(model.FullName).SemiBold();
                            column.Item().Text($"User ID. {model.UserId}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            column.Item().Text($"Email. {model.Email}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("PLAN DETAIL").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken1);
                            column.Item().PaddingTop(4).Text(model.PlanName).SemiBold();
                            column.Item().Text($"Period. {DescribePeriod(model)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            column.Item().Text($"Active from. {FormatDate(model.ActivatedFrom)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            column.Item().Text($"Valid until. {DescribeValidity(model.ActivatedTo)}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    content.Item().PaddingTop(16).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(110);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                .Text("DESCRIPTION").SemiBold().FontSize(8);
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                .AlignRight().Text("AMOUNT").SemiBold().FontSize(8);
                        });

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6)
                            .Column(column =>
                            {
                                column.Item().Text($"{model.PlanName} plan").SemiBold();
                                column.Item().Text($"Request No. {model.RequestNo}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });

                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6)
                            .AlignRight().Text(FormatAmount(model.Amount, model.Currency)).SemiBold();
                    });

                    content.Item().PaddingTop(8).AlignRight().Width(220).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(110);
                        });

                        table.Cell().PaddingVertical(3).Text("Subtotal").FontSize(9);
                        table.Cell().PaddingVertical(3).AlignRight()
                            .Text(FormatAmount(model.Amount, model.Currency)).FontSize(9);

                        table.Cell().PaddingVertical(3).Text("Total").SemiBold();
                        table.Cell().PaddingVertical(3).AlignRight()
                            .Text(FormatAmount(model.Amount, model.Currency)).SemiBold().FontSize(11);
                    });

                    content.Item().PaddingTop(18).Column(notes =>
                    {
                        notes.Item().Text("NOTES").FontSize(8).SemiBold().FontColor(Colors.Grey.Darken1);
                        notes.Item().PaddingTop(4).Text($"Payment method. {model.PaymentMethod}").FontSize(8);
                        notes.Item().Text($"Payment reference. {model.TransferReference ?? "-"}").FontSize(8);
                        notes.Item().Text($"Payment request. {model.RequestNo}").FontSize(8);
                    });
                });

                page.Footer().BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(6)
                    .Text("This invoice was issued automatically by the DeckZa system.")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    /// <summary>Format uang <c>id-ID</c> tanpa desimal, mis. <c>Rp 49.000</c>.</summary>
    private static string FormatAmount(decimal amount, string currency)
    {
        var prefix = string.Equals(currency, "IDR", StringComparison.OrdinalIgnoreCase) ? "Rp " : $"{currency} ";
        return prefix + amount.ToString("N0", IndonesianCulture);
    }

    private static string FormatDate(DateTime? value)
        => value.HasValue ? value.Value.ToString("dd MMM yyyy", IndonesianCulture) : "-";

    /// <summary>
    /// Masa berlaku: <c>Selamanya</c> bila <c>activated_to</c> kosong,
    /// bila terisi tampilkan tanggal berakhirnya.
    /// </summary>
    private static string DescribeValidity(DateTime? activatedTo)
        => activatedTo.HasValue ? activatedTo.Value.ToString("dd MMM yyyy", IndonesianCulture) : "Selamanya";

    /// <summary>
    /// Periode plan. Untuk plan bulanan ditampilkan rentang tanggal aktif.
    /// </summary>
    private static string DescribePeriod(InvoiceRenderModel model)
    {
        if (!string.Equals(model.BillingPeriod, "MONTHLY", StringComparison.OrdinalIgnoreCase))
        {
            return model.BillingPeriod == "YEARLY" ? "Yearly" : "One time";
        }

        if (!model.ActivatedFrom.HasValue)
        {
            return "Monthly";
        }

        var start = model.ActivatedFrom.Value;
        var end = model.ActivatedTo ?? start.AddMonths(1);
        return $"Monthly · {start.ToString("d MMM", IndonesianCulture)} – {end.ToString("d MMM yyyy", IndonesianCulture)}";
    }
}
