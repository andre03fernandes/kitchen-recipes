namespace KitchenRecipes.Infrastructure.Configuration;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "none";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.1:8b";
    public string ActiveProfile { get; set; } = "fast";
    public string FastModel { get; set; } = "llama3.1:8b";
    public string QualityModel { get; set; } = "llama3.1:70b";
    public string? ApiKey { get; set; }

    public string ResolveModel()
    {
        if (string.Equals(ActiveProfile, "quality", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(QualityModel))
        {
            return QualityModel;
        }

        if (string.Equals(ActiveProfile, "fast", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(FastModel))
        {
            return FastModel;
        }

        return Model;
    }
}
