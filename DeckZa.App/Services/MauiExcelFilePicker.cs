using Microsoft.Maui.Storage;

using DeckZa.Shared.Services.Interfaces;

namespace DeckZa.App.Services;

public class MauiExcelFilePicker : IExcelFilePicker
{
    public bool IsNativePickerSupported => true;

    public async Task<ExcelFilePickResult?> PickAsync(CancellationToken cancellationToken = default)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Excel file",
        });
        if (result == null)
        {
            return null;
        }

        if (!result.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return new ExcelFilePickResult(result.FileName, Array.Empty<byte>());
        }

        await using var stream = await result.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        if (memory.Length > 10 * 1024 * 1024)
        {
            return new ExcelFilePickResult(result.FileName, Array.Empty<byte>());
        }

        return new ExcelFilePickResult(result.FileName, memory.ToArray());
    }
}
