using BlazorMerge.Feature.Merge;
using BlazorMerge.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BlazorMerge.UnitTests.Merge;

public class MergeServiceTests
{
    [Theory]
    [InlineData("appsettings.json", "appsettings.{environment}.json", "appsettings.Development.json")]
    [InlineData("bob.json", "phil.{environment}.json", "phil.Development.json")]
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
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(m => m["AppSettings:MainFileName"]).Returns(primaryFileName);
        mockConfig.Setup(m => m["AppSettings:EnvironmentFileName"]).Returns(secondaryFileFormat);
        
        var mockFileManager = new Mock<IFileManager>();
        mockFileManager.Setup(m => m.ReadFile(It.IsAny<string>())).Returns(readValue);
        mockFileManager.Setup(m => m.WriteFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        mockFileManager.Setup(m => m.ListSettingsFiles(It.IsAny<string>(), It.IsAny<Func<string, bool>>())).Returns(settingsFiles);
        mockFileManager.Setup(m => m.DeleteFile(It.IsAny<string>())).Verifiable();
        
        var mockMerger = new Mock<IMerger>();
        mockMerger.Setup(m => m.Merge(It.IsAny<string>(), It.IsAny<string>())).Returns(mergeValue);
        
        var mergeService = new MergeService(
            mockConfig.Object,
            mockFileManager.Object,
            mockMerger.Object
            );
        
        // Act
        var result = mergeService.MergeEnvironment(options);
        
        // Assert
        result.Should().Be(0);
        mockFileManager.Verify(m => m.ReadFile(It.IsAny<string>()), Times.Exactly(2));
        mockFileManager.Verify(m => m.ReadFile($"{options.Path}{primaryFileName}"), Times.Once);
        mockFileManager.Verify(m => m.ReadFile($"{options.Path}{secondaryFileFinalFormat}"), Times.Once);
        mockFileManager.Verify(m => m.WriteFile($"{options.Path}{primaryFileName}", mergeValue), Times.Once);
        
        var environmentJsonFiles = settingsFiles.Where(s => s == secondaryFileFinalFormat).ToList();
        var allBrotliFiles = settingsFiles.Where(s => s.EndsWith(".br")).ToList();
        var allGzipFiles = settingsFiles.Where(s => s.EndsWith(".gz")).ToList();
        var filesToDeleteCount = environmentJsonFiles.Count + allBrotliFiles.Count + allGzipFiles.Count;

        mockFileManager.Verify(m => m.ListSettingsFiles(options.Path, It.IsAny<Func<string, bool>>()), Times.Once);
        mockFileManager.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Exactly(filesToDeleteCount));
        VerifyDeletion(mockFileManager, options, environmentJsonFiles);
        VerifyDeletion(mockFileManager, options, allBrotliFiles);
        VerifyDeletion(mockFileManager, options, allGzipFiles);
        
        mockMerger.Verify(m => m.Merge(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static void VerifyDeletion(Mock<IFileManager> mockFileManager, MergeOptions options, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            var path = $"{options.Path}{file}";
            mockFileManager.Verify(m => m.DeleteFile(path), Times.Once);
        }
    }
}