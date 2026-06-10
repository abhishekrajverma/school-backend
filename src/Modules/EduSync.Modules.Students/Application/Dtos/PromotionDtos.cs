namespace EduSync.Modules.Students.Application.Dtos;

public sealed record PromotionRuleDto(string FromClass, string ToClass);

public sealed record BulkPromoteRequest(
    Guid FromAcademicYearId,
    Guid ToAcademicYearId,
    IReadOnlyList<PromotionRuleDto> Rules);

public sealed record PromotionResultDto(
    string BatchId,
    string Status,
    int TotalStudents,
    int PromotedCount,
    int SkippedCount);
