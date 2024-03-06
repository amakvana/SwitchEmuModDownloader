namespace SwitchEmuModDownloader.Classes.Utilities;

public static class DirectoryUtilities
{
    // https://stackoverflow.com/a/36925751
    public static async Task CopySingleFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken = default)
    {
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        const int bufferSize = 8192;
        await using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
        await using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);
        await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
    }

    // https://stackoverflow.com/a/49570235
    public static void CopyAllFiles(string fromFolder, string toFolder, bool overwrite = false) =>
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
