using System.Globalization;
using System.Security.Cryptography;

using ClosedXML.Excel;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.Tasks;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Services;

public class TaskImportService : ITaskImportService
{
    private const long MaxFileSize = 1024 * 1024;
    private const string TransactionSheetName = "Transaksi";
    private const string ReferenceSheetName = "Master Referensi";

    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd",
        "dd/MM/yyyy HH:mm",
        "dd/MM/yyyy",
        "dd-MM-yyyy HH:mm",
        "dd-MM-yyyy",
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;
    private readonly IWebHostEnvironment _environment;

    public TaskImportService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IErrorLogService errorLog,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _errorLog = errorLog;
        _environment = environment;
    }

    public async Task<TaskImportTemplateDto> BuildTemplateAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var applications = await _unitOfWork.TaskImports.GetActiveApplicationsAsync(userId, cancellationToken);
        var statuses = await _unitOfWork.TaskImports.GetActiveStatusesAsync(userId, cancellationToken);
        var taskTypes = await _unitOfWork.TaskImports.GetActiveTaskTypesAsync(userId, cancellationToken);
        var priorities = await _unitOfWork.TaskImports.GetActiveTaskPrioritiesAsync(userId, cancellationToken);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(TransactionSheetName);

        var headers = new[]
        {
            "aplikasi", "title", "description", "request_by", "request_date",
            "due_date", "completed_date", "status", "task_type", "priority",
        };

        for (var column = 0; column < headers.Length; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F0FE");
        }

        var exampleStatus = statuses.FirstOrDefault();
        var exampleType = taskTypes.FirstOrDefault();
        var examplePriority = priorities.FirstOrDefault();
        var exampleApplication = applications.FirstOrDefault();

        var example = new object?[]
        {
            exampleApplication?.description ?? "Aplikasi",
            "Contoh task (hapus baris ini sebelum upload)",
            "Baris ini hanya contoh pengisian.",
            "Nama Peminta",
            "2026-11-01 09:00",
            string.Empty,
            string.Empty,
            exampleStatus?.description ?? "Draft",
            exampleType?.description ?? "Enhance",
            examplePriority?.description ?? "Low",
        };

        for (var column = 0; column < example.Length; column++)
        {
            sheet.Cell(2, column + 1).Value = example[column]?.ToString() ?? string.Empty;
        }

        sheet.Cell(1, 5).CreateComment().AddText("Format tanggal: yyyy-MM-dd HH:mm (contoh 2026-11-01 09:00).");
        sheet.Cell(1, 6).CreateComment().AddText("Opsional. Format: yyyy-MM-dd HH:mm.");
        sheet.Cell(1, 7).CreateComment().AddText("Opsional, hanya untuk status Done. Format: yyyy-MM-dd HH:mm.");
        sheet.Columns().AdjustToContents();
        ClampColumnWidths(sheet);

        var reference = workbook.Worksheets.Add(ReferenceSheetName);
        var row = 1;

        var applicationRange = WriteReferenceBlock(reference, ref row, "Aplikasi ID", "Aplikasi",
            applications.Select(item => (item.application_id, item.description)));
        var taskTypeRange = WriteReferenceBlock(reference, ref row, "Task Type ID", "Task Type",
            taskTypes.Select(item => (item.task_type_id, item.description)));
        var priorityRange = WriteReferenceBlock(reference, ref row, "Priority ID", "Priority",
            priorities.Select(item => (item.task_priority_id, item.description)));
        var statusRange = WriteReferenceBlock(reference, ref row, "Status ID", "Status",
            statuses.Select(item => (item.status_id, item.description)));

        reference.Cell(row + 1, 1).Value =
            "Catatan: baris contoh pada sheet Transaksi harus dihapus sebelum upload. "
            + "Status STS001 (Draft) dan STS005 (Done) adalah status sistem.";
        reference.Range(row + 1, 1, row + 1, 2).Merge();

        sheet.Range(2, 1, 1000, 1).CreateDataValidation().List(applicationRange);
        sheet.Range(2, 8, 1000, 8).CreateDataValidation().List(statusRange);
        sheet.Range(2, 9, 1000, 9).CreateDataValidation().List(taskTypeRange);
        sheet.Range(2, 10, 1000, 10).CreateDataValidation().List(priorityRange);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new TaskImportTemplateDto
        {
            Content = stream.ToArray(),
            FileName = "task_import_template.xlsx",
        };
    }

    public async Task<TaskImportOutcomeDto> ImportAsync(
        Stream stream,
        string fileName,
        long length,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Fail("User session is not valid.");
        }

        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Only .xlsx files are supported.");
        }

        if (length <= 0)
        {
            return Fail("The uploaded file is empty.");
        }

        if (length > MaxFileSize)
        {
            return Fail("The uploaded file exceeds the 1 MB limit.");
        }

        string? tempPath = null;

        try
        {
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            var bytes = memory.ToArray();

            if (bytes.Length == 0)
            {
                return Fail("The uploaded file is empty.");
            }

            if (bytes.Length > MaxFileSize)
            {
                return Fail("The uploaded file exceeds the 1 MB limit.");
            }

            var fileHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            var existingBatch = await _unitOfWork.TaskImports.GetBatchByHashAsync(fileHash, userId, cancellationToken);
            if (existingBatch != null)
            {
                return Fail($"This file has already been imported (batch {existingBatch.import_batch_id}).");
            }

            var tempFolder = Path.Combine(_environment.ContentRootPath, "tmp");
            Directory.CreateDirectory(tempFolder);
            CleanupStaleTempFiles(tempFolder);
            tempPath = Path.Combine(tempFolder, $"task-import-{Guid.NewGuid():N}.xlsx");
            await File.WriteAllBytesAsync(tempPath, bytes, cancellationToken);

            using var workbook = new XLWorkbook(tempPath);
            var sheet = workbook.Worksheets.FirstOrDefault(item => item.Name == TransactionSheetName)
                        ?? workbook.Worksheets.First();

            var applications = await _unitOfWork.TaskImports.GetActiveApplicationsAsync(userId, cancellationToken);
            var statuses = await _unitOfWork.TaskImports.GetActiveStatusesAsync(userId, cancellationToken);
            var taskTypes = await _unitOfWork.TaskImports.GetActiveTaskTypesAsync(userId, cancellationToken);
            var priorities = await _unitOfWork.TaskImports.GetActiveTaskPrioritiesAsync(userId, cancellationToken);

            var applicationMap = BuildLookup(
                applications.Select(item => (item.application_id, item.description)));
            var statusMap = BuildLookup(
                statuses.Select(item => (item.status_id, item.description)));
            var taskTypeMap = BuildLookup(
                taskTypes.Select(item => (item.task_type_id, item.description)));
            var priorityMap = BuildLookup(
                priorities.Select(item => (item.task_priority_id, item.description)));

            var now = DateTime.Now;
            var batchId = Guid.NewGuid();
            var tasks = new List<tb_task>();
            var rejections = new List<TaskImportRejectionDto>();
            var totalRows = 0;

            foreach (var sheetRow in sheet.RowsUsed().Where(item => item.RowNumber() >= 2))
            {
                var rowNumber = sheetRow.RowNumber();
                if (IsBlankRow(sheet, rowNumber))
                {
                    continue;
                }

                totalRows++;
                var errors = new List<string>();

                var applicationName = ReadText(sheet, rowNumber, 1);
                var title = ReadText(sheet, rowNumber, 2);
                var description = ReadText(sheet, rowNumber, 3);
                var requestBy = ReadText(sheet, rowNumber, 4);
                var statusName = ReadText(sheet, rowNumber, 8);
                var taskTypeName = ReadText(sheet, rowNumber, 9);
                var priorityName = ReadText(sheet, rowNumber, 10);

                if (string.IsNullOrWhiteSpace(title))
                {
                    errors.Add("title is required.");
                }

                if (string.IsNullOrWhiteSpace(requestBy))
                {
                    errors.Add("request_by is required.");
                }

                TryResolveId(applicationMap, applicationName, "aplikasi", errors, out var applicationId);
                TryResolveId(statusMap, statusName, "status", errors, out var statusId);
                TryResolveId(taskTypeMap, taskTypeName, "task_type", errors, out var taskTypeId);
                TryResolveId(priorityMap, priorityName, "priority", errors, out var priorityId);

                if (!TryReadDate(sheet, rowNumber, 5, required: true, "request_date", errors, out var requestDate))
                {
                    requestDate = null;
                }

                TryReadDate(sheet, rowNumber, 6, required: false, "due_date", errors, out var dueDate);
                TryReadDate(sheet, rowNumber, 7, required: false, "completed_date", errors, out var completedDate);

                var completedStatus = statuses.FirstOrDefault(e => e.status_id == statusId)?.is_completed_status == true;
                if (completedDate.HasValue && !completedStatus)
                {
                    errors.Add("completed_date is only allowed for a Completed status.");
                }

                if (errors.Count > 0)
                {
                    rejections.Add(new TaskImportRejectionDto
                    {
                        RowNumber = rowNumber,
                        Message = string.Join(" ", errors),
                    });
                    continue;
                }

                var isCompleted = statuses.FirstOrDefault(e => e.status_id == statusId)?.is_completed_status == true;
                tasks.Add(new tb_task
                {
                    id = Guid.NewGuid(),
                    application_id = applicationId,
                    title = title!,
                    description = string.IsNullOrWhiteSpace(description) ? null : description,
                    request_by = requestBy!,
                    request_date = requestDate!.Value,
                    due_date = dueDate,
                    completed_date = isCompleted ? completedDate : null,
                    status_id = statusId,
                    task_type_id = taskTypeId,
                    task_priority_id = priorityId,
                    import_batch_id = batchId,
                    statusApproval = Const.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = false,
                    createdBy = userId,
                    createdDate = now,
                    approvedBy = userId,
                    approvedDate = now,
                    approvalTypeId = Const.ApprovalType.ADD,
                });
            }

            var batch = new tb_import_batch
            {
                import_batch_id = batchId,
                file_name = fileName,
                file_hash = fileHash,
                import_by = userId,
                import_date = now,
                total_rows = totalRows,
                total_success = tasks.Count,
                total_rejected = rejections.Count,
                reject_summary = BuildRejectSummary(rejections),
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
                approvedBy = userId,
                approvedDate = now,
                approvalTypeId = Const.ApprovalType.ADD,
            };

            await _unitOfWork.TaskImports.AddBatchAsync(batch, cancellationToken);
            foreach (var task in tasks)
            {
                await _unitOfWork.TaskImports.AddTaskAsync(task, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TaskImportOutcomeDto
            {
                Success = true,
                Message = tasks.Count == 0
                    ? "Import completed but every row was rejected."
                    : "Import completed successfully.",
                Result = new TaskImportResultDto
                {
                    ImportBatchId = batchId,
                    TotalRows = totalRows,
                    SuccessCount = tasks.Count,
                    RejectedCount = rejections.Count,
                    Rejections = rejections,
                },
            };
        }
        catch (DbUpdateException exception)
        {
            await LogErrorAsync(exception);
            return Fail("This file could not be imported. It may have already been imported by another request.");
        }
        catch (Exception exception)
        {
            await LogErrorAsync(exception);
            return Fail("The file could not be read as a valid .xlsx workbook.");
        }
        finally
        {
            if (tempPath != null)
            {
                try
                {
                    System.IO.File.Delete(tempPath);
                }
                catch
                {
                    // Temporary file cleanup must not fail the import result.
                }
            }
        }
    }

    private static IXLRange WriteReferenceBlock(
        IXLWorksheet sheet,
        ref int row,
        string idHeader,
        string nameHeader,
        IEnumerable<(string Id, string Name)> items)
    {
        sheet.Cell(row, 1).Value = idHeader;
        sheet.Cell(row, 2).Value = nameHeader;
        sheet.Cell(row, 1).Style.Font.Bold = true;
        sheet.Cell(row, 2).Style.Font.Bold = true;
        row++;

        var startRow = row;
        foreach (var item in items)
        {
            sheet.Cell(row, 1).Value = item.Id;
            sheet.Cell(row, 2).Value = item.Name ?? item.Id;
            row++;
        }

        var endRow = row - 1;
        var range = sheet.Range(startRow, 2, Math.Max(endRow, startRow), 2);
        row += 2;
        return range;
    }

    private static Dictionary<string, string> BuildLookup(IEnumerable<(string Id, string Name)> items)
    {
        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                continue;
            }

            lookup[item.Name.Trim()] = item.Id;
        }

        return lookup;
    }

    private static bool TryResolveId(
        Dictionary<string, string> lookup,
        string? name,
        string field,
        List<string> errors,
        out string id)
    {
        id = string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add($"{field} is required.");
            return false;
        }

        if (!lookup.TryGetValue(name.Trim(), out var resolved) || resolved is null)
        {
            errors.Add($"{field} \"{name.Trim()}\" is not recognized or inactive.");
            return false;
        }

        id = resolved;
        return true;
    }

    private static bool TryReadDate(
        IXLWorksheet sheet,
        int rowNumber,
        int column,
        bool required,
        string field,
        List<string> errors,
        out DateTime? value)
    {
        value = null;
        var cell = sheet.Cell(rowNumber, column);

        if (cell.IsEmpty())
        {
            if (required)
            {
                errors.Add($"{field} is required.");
            }

            return !required;
        }

        if (cell.DataType == XLDataType.DateTime)
        {
            value = cell.GetDateTime();
            return true;
        }

        var text = cell.GetFormattedString().Trim();
        if (DateTime.TryParseExact(
                text,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed)
            || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
            || DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
        {
            value = parsed;
            return true;
        }

        errors.Add($"{field} is invalid (use yyyy-MM-dd HH:mm).");
        return false;
    }

    private static string? ReadText(IXLWorksheet sheet, int rowNumber, int column)
    {
        var cell = sheet.Cell(rowNumber, column);
        if (cell.IsEmpty())
        {
            return null;
        }

        var text = cell.GetFormattedString().Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool IsBlankRow(IXLWorksheet sheet, int rowNumber)
    {
        for (var column = 1; column <= 10; column++)
        {
            if (!sheet.Cell(rowNumber, column).IsEmpty())
            {
                return false;
            }
        }

        return true;
    }

    private static void CleanupStaleTempFiles(string folder)
    {
        try
        {
            var threshold = DateTime.UtcNow.AddHours(-1);
            foreach (var file in Directory.EnumerateFiles(folder, "task-import-*.xlsx"))
            {
                try
                {
                    if (System.IO.File.GetLastWriteTimeUtc(file) < threshold)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch
                {
                    // A locked stale file can be retried on the next import.
                }
            }
        }
        catch
        {
            // Temporary folder cleanup must not block the import.
        }
    }

    private static string? BuildRejectSummary(List<TaskImportRejectionDto> rejections)
    {
        if (rejections.Count == 0)
        {
            return null;
        }

        var summary = string.Join(
            " | ",
            rejections.Take(20).Select(item => $"Row {item.RowNumber}: {item.Message}"));

        return summary.Length > 1000 ? summary[..1000] : summary;
    }

    private static void ClampColumnWidths(IXLWorksheet sheet)
    {
        foreach (var column in sheet.ColumnsUsed())
        {
            if (column.Width > 45)
            {
                column.Width = 45;
            }
        }
    }

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("TaskImport", "ERROR", exception);

    private static TaskImportOutcomeDto Fail(string message) => new()
    {
        Success = false,
        Message = message,
    };
}
