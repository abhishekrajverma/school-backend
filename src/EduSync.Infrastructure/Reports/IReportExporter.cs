namespace EduSync.Infrastructure.Reports;

public interface IReportExporter
{
    Task<(byte[] Content, string FileName, string ContentType)> ExportAsync(
        string reportType,
        string format,
        CancellationToken cancellationToken = default);
}
