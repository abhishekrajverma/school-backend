using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Application.Commands;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Identity;

public sealed class LogoutCommandHandler(EduSyncDbContext db, IJwtTokenService jwtTokenService)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var hash = jwtTokenService.HashToken(request.RefreshToken);
            var token = await db.RefreshTokens
                .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.TokenHash == hash, cancellationToken);
            if (token is not null)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var tokens = await db.RefreshTokens
                .Where(r => r.UserId == request.UserId && r.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
