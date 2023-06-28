using CommandLine;

namespace BlazorMerge.Feature.Merge;

[Verb("merge", HelpText = "Merges two appsettings.json files into one.")]
public class MergeOptions
{
    [Option('e', "environment",
        Required = true,
        HelpText = "The environment you are deploying to.")]
    public string Environment { get; set; } = null!;

    [Option ('p', "path",
        Required = true,
        HelpText = "The path to the environment appsettings.json file.")]
    public string Path { get; set; } = null!;
    
    [Option('h', "help",
        Default = false,
        HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }
}