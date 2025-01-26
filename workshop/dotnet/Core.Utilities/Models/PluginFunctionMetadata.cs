namespace Core.Utilities.Models;
   
public record PluginFunctionMetadata(
    string Name,
    string Description,
    List<PluginParameterMetadata> Parameters
);

public record PluginParameterMetadata(
    string Name,
    string Type,
    string Description,
    object? DefaultValue
);
