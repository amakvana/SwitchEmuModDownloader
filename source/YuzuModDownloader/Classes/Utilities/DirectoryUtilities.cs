namespace YuzuModDownloader.Classes.Utilities
{
    public static class DirectoryUtilities
    {
        // https://stackoverflow.com/a/36925751
        public static async Task CopySingleFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken = default)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 8192;
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
            using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
            await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
        }

        public static void CopyAllFiles(string fromFolder, string toFolder, bool overwrite = false)
        {
            // https://stackoverflow.com/a/49570235
            Directory
                .EnumerateFiles(fromFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => (File.GetAttributes(file) & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                .AsParallel()
                .ForAll(from =>
                {
                    var to = from.Replace(fromFolder, toFolder);

                    // Create directories if required
                    var toSubFolder = Path.GetDirectoryName(to);
                    if (!string.IsNullOrWhiteSpace(toSubFolder))
                    {
                        Directory.CreateDirectory(toSubFolder);
                    }

                    File.Copy(from, to, overwrite);
                });
        }
    }
}
