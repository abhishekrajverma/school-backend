using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Library.Domain;

public sealed class BookIssue : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public string BookExternalId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string MemberExternalId { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string MemberType { get; set; } = "student";
    public string? ClassName { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string Status { get; set; } = "issued";
    public decimal Fine { get; set; }

    public Book Book { get; set; } = null!;
}
