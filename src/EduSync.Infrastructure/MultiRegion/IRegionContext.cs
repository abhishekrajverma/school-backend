namespace EduSync.Infrastructure.MultiRegion;

public interface IRegionContext
{
    string? CurrentRegion { get; }
    void Set(string region);
}
