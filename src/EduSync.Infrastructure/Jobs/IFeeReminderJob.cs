namespace EduSync.Infrastructure.Jobs;

public interface IFeeReminderJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
