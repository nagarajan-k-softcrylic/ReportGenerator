namespace ReportGenerator.Infrastructure.Storage;

public class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "reports";
    /// <summary>How long the generated SAS download URL remains valid.</summary>
    public int SasExpiryHours { get; set; } = 24;
}
