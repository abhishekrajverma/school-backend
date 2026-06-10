using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Students.Domain;

public sealed class PromotionBatchItem : TenantEntity
{
    public Guid Id { get; set; }
    public Guid PromotionBatchId { get; set; }
    public Guid StudentId { get; set; }
    public Guid FromEnrollmentId { get; set; }
    public Guid? ToEnrollmentId { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? SkipReason { get; set; }

    public PromotionBatch Batch { get; set; } = null!;
}

public static class PromotionOutcomes
{
    public const string Promoted = "promoted";
    public const string SkippedInactive = "skipped_inactive";
    public const string SkippedNoTargetClass = "skipped_no_target_class";
}
