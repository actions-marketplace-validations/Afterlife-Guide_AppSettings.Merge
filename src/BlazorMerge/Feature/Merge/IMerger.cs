namespace BlazorMerge.Feature.Merge;

public interface IMerger
{
    string Merge(string appSetting, string environmentSetting);
}