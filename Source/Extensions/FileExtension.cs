using System.IO;

namespace DiscordBotRewrite.Extensions {
    public static class FileExtension {
        //Creates all sub directories needed and then the file
        public static void CreateFileWithPath(string path) {
            if(string.IsNullOrWhiteSpace(path)) return;

            string directoryPath = Path.GetDirectoryName(path);
            if(!string.IsNullOrWhiteSpace(directoryPath)) {
                if(!Directory.Exists(directoryPath)) {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            File.Create(path).Dispose();
        }
    }
}