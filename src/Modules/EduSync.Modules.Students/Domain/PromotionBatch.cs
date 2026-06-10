using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed class PromotionBatch : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid FromAcademicYearId { get; set; }
    public Guid ToAcademicYearId { get; set; }
    public string Status { get; set; } = PromotionBatchStatuses.Completed;
    public int TotalStudents { get; set; }
    public int PromotedCount { get; set; }
    public int SkippedCount { get; set; }
    public Guid? ExecutedByUserId { get; set; }
    public DateTime ExecutedAt { get; set; }
    public DateTime? RolledBackAt { get; set; }

    public ICollection<PromotionBatchItem> Items { get; set; } = [];
}

public static class PromotionBatchStatuses
{
    public const string Completed = "completed";
    public const string RolledBack = "rolled_back";
}
