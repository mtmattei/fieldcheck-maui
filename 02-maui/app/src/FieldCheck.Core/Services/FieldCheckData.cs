using System.Text.Json.Serialization;
using FieldCheck.Core.Models;

namespace FieldCheck.Core.Services;

/// <summary>Shape of the local data file persisted in app storage.</summary>
public sealed class FieldCheckData
{
    public List<Asset> Assets { get; set; } = [];
    public List<Inspection> Inspections { get; set; } = [];
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FieldCheckData))]
[JsonSerializable(typeof(List<Asset>))]
[JsonSerializable(typeof(List<Inspection>))]
internal sealed partial class FieldCheckJsonContext : JsonSerializerContext;
