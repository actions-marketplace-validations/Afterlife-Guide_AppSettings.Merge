namespace BlazorMerge.Files;

public class SettingsFile
{
    public string FileName { get; set; } = null!;
    public SettingsFileType Type { get; set; }
    public SettingsFileExtension Extension { get; set; }
}