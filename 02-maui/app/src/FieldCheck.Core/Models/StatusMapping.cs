namespace FieldCheck.Core.Models;

public static class StatusMapping
{
    public static AssetStatus ToAssetStatus(this InspectionCondition condition) => condition switch
    {
        InspectionCondition.Good => AssetStatus.Operational,
        InspectionCondition.Attention => AssetStatus.Attention,
        _ => AssetStatus.Critical,
    };
}
