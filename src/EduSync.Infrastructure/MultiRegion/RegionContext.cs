namespace EduSync.Infrastructure.MultiRegion;

public sealed class RegionContext : IRegionContext
{
    public string? CurrentRegion { get; private set; }
    public void Set(string region) => CurrentRegion = region;
}
