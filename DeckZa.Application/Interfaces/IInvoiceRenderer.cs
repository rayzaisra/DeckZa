using DeckZa.Application.DTOs.Payments;

namespace DeckZa.Application.Interfaces;

/// <summary>
/// Merender invoice menjadi file PDF.
/// Implementasi memakai QuestPDF dan berada di project Api (QuestPDF hanya
/// direference di sana), sedangkan service invoice berada di Infrastructure.
/// Karena itu kontrak ini didefinisikan di Application.
/// </summary>
public interface IInvoiceRenderer
{
    /// <summary>Mengembalikan isi file PDF dalam bentuk byte[].</summary>
    byte[] Render(InvoiceRenderModel model);
}
