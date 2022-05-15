namespace DiscordBotRewrite.Extensions;

public static class FileExtension {
    //Creates all sub directories needed and then the file
    public static void CreateFileWithPath(string path) {
        string directoryPath = Path.GetDirectoryName(path);
        if(!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        File.Create(path).Dispose();
    } 
}
