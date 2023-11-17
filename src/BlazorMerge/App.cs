using BlazorMerge.Feature.Merge;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace BlazorMerge;

public class App
{
    private readonly MergeService _mergeService;

    public App(IConfiguration configuration, MergeService mergeService)
    {
        _mergeService = mergeService;
    }

    public void Run(IEnumerable<string> args)
    {
        Parser.Default.ParseArguments<MergeOptions>(args)
            .MapResult(
                (MergeOptions opts) => _mergeService.MergeEnvironment(opts),
                errs => 1
                );
    }
}