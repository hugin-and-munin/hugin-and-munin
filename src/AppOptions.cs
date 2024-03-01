namespace HuginAndMunin;

public record AppOptions
{
    public const string Name = "AppOptions";
    public required string LegalEntitiesCheckerUri { get; init; }
    public required string CredCheckerUri { get; init; }
}
