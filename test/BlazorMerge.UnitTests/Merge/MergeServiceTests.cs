using BlazorMerge.Feature.Merge;
using BlazorMerge.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace BlazorMerge.UnitTests.Merge;

public class MergeServiceTests
{
    [Theory]
    [InlineData("appsettings.json", "appsettings.{environment}.json", "appsettings.Development.json")]
    public void When_LoadingData_Then_DataShouldBePassedToMergerCorrectly(string primaryFileName, string secondaryFileFormat, string secondaryFileFinalFormat)
    {
        // Arrange
        var options = new MergeOptions
        {
            Environment = "Development",
            Path = @"Z:\my-project\wwwroot\"
        };
        
        const string readValue = "{\"key\": \"value\"}";
        const string mergeValue = "{\"key\": \"other value\"}";
        
        IList<string> settingsFiles = new List<string>
        {
            primaryFileName,
            $"{primaryFileName}.br",
            $"{primaryFileName}.gz",
            secondaryFileFinalFormat,
            $"{secondaryFileFinalFormat}.br",
            $"{secondaryFileFinalFormat}.gz"
        };
        var mockConfig = Substitute.For<IConfiguration>();
        mockConfig["AppSettings:MainFileName"].Returns(primaryFileName);
        mockConfig["AppSettings:EnvironmentFileName"].Returns(secondaryFileFormat);

        var mockFileManager = Substitute.For<IFileManager>();
        mockFileManager.ReadFile(Arg.Any<string>()).Returns(readValue);
        mockFileManager.WriteFile(Arg.Any<string>(), Arg.Any<string>()); // can I delete this line?
        mockFileManager.ListSettingsFiles(Arg.Any<string>(), Arg.Any<Func<string, bool>>()).Returns(settingsFiles);
        mockFileManager.DeleteFile(Arg.Any<string>()); // can I delete this line?
        
        var mockMerger = Substitute.For<IMerger>();
        mockMerger.Merge(Arg.Any<string>(), Arg.Any<string>()).Returns(mergeValue);
        
        var mergeService = new MergeService(
            mockConfig,
            mockFileManager,
            mockMerger
            );
        
        // Act
        var result = mergeService.MergeEnvironment(options);
        
        // Assert
        result.Should().Be(0);
        mockFileManager.Received(2).ReadFile(Arg.Any<string>());
        mockFileManager.Received(1).ReadFile($"{options.Path}{primaryFileName}");
        mockFileManager.Received(1).ReadFile($"{options.Path}{secondaryFileFinalFormat}");
        mockFileManager.Received(1).WriteFile($"{options.Path}{primaryFileName}", mergeValue);
        
        var environmentJsonFiles = settingsFiles.Where(s => s == secondaryFileFinalFormat).ToList();
        var allBrotliFiles = settingsFiles.Where(s => s.EndsWith(".br")).ToList();
        var allGzipFiles = settingsFiles.Where(s => s.EndsWith(".gz")).ToList();
        var filesToDeleteCount = environmentJsonFiles.Count + allBrotliFiles.Count + allGzipFiles.Count;

        mockFileManager.Received(1).ListSettingsFiles(options.Path, Arg.Any<Func<string, bool>>());
        mockFileManager.Received(filesToDeleteCount).DeleteFile(Arg.Any<string>());
        VerifyDeletion(mockFileManager, options, environmentJsonFiles);
        VerifyDeletion(mockFileManager, options, allBrotliFiles);
        VerifyDeletion(mockFileManager, options, allGzipFiles);
        
        mockMerger.Received(1).Merge(Arg.Any<string>(), Arg.Any<string>());
    }

    private static void VerifyDeletion(IFileManager mockFileManager, MergeOptions options, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            var path = $"{options.Path}{file}";
            mockFileManager.Received(1).DeleteFile(path);
        }
    }
}