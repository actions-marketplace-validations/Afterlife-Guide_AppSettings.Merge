namespace BlazorMerge.Files;

public class FileManager : IFileManager
{
    public string ReadFile(string path)
    {
        if (FileDoesNotExist(path))
        {
            throw new FileNotFoundException($"File {path} does not exist");
        }
        
        return File.ReadAllText(path);
    }

    private static bool FileDoesNotExist(string path)
    {
        return !File.Exists(path);
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public IList<string> ListSettingsFiles(string path, Func<string, bool> parser)
    {
        var files = Directory.GetFiles(path);

        return (from file in files
            where parser(file)
            select Path.GetFileName(file)).ToList();
    }
}