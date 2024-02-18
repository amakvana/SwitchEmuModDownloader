namespace YuzuModDownloader.Classes.Extensions;

// https://stackoverflow.com/a/46497896
public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // create a 3 second cancellation token for GET requests 
            using var requestCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            // Get the http headers first to examine the content length
            using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, requestCancellationTokenSource.Token);
            var contentLength = response.Content.Headers.ContentLength;
            await using var download = await response.Content.ReadAsStreamAsync(cancellationToken);

            // Ignore progress reporting when no progress reporter was 
            // passed or when the content length is unknown
            if (progress is null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
                return;
            }

            // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
            var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
            // Use extension method to report progress while downloading
            await download.CopyToAsync(destination, 8192, relativeProgress, cancellationToken);
            progress.Report(1);
        }
        catch { }
    }
}
