namespace DeckZa.Shared.Services;

using DeckZa.Shared.Services.Interfaces;

public class NoopExcelFilePicker : IExcelFilePicker
{
    public bool IsNativePickerSupported => false;

    public Task<ExcelFilePickResult?> PickAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ExcelFilePickResult?>(null);
}
