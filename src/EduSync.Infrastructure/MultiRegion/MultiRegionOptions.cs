namespace EduSync.Infrastructure.MultiRegion;

public sealed class MultiRegionOptions
{
    public string CurrentRegion { get; set; } = "ap-south-1";
    public string[] AllowedRegions { get; set; } = ["ap-south-1", "us-east-1", "eu-west-1"];
    public bool RequireRegionHeader { get; set; }
}
