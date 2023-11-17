using Newtonsoft.Json.Linq;

namespace BlazorMerge.Feature.Merge;

public class Merger : IMerger
{
    public string Merge(string appSetting, string environmentSetting)
    {
        var appSettingJObject = JObject.Parse(appSetting);
        var environmentSettingJObject = JObject.Parse(environmentSetting);
        appSettingJObject.Merge(environmentSettingJObject, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            
        });
        return appSettingJObject.ToString();
    }
}