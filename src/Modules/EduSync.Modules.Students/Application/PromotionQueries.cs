using EduSync.Modules.Students.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Students.Application;

public sealed record BulkPromoteStudentsCommand(BulkPromoteRequest Request) : IRequest<Result<PromotionResultDto>>;
public sealed record RollbackPromotionBatchCommand(string BatchExternalId) : IRequest<Result<PromotionResultDto>>;
