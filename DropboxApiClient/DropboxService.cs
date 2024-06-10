using Dropbox.Api;
using Dropbox.Api.Files;

namespace DropboxApiClient;

public class DropboxService
{
    private const int ChunkSize = 1 * 1024 * 1024; // 1MB

    public static async Task<IEnumerable<Metadata>> ListFolder(DropboxClient client, string path)
    {
        var files = new List<Metadata>();
        var result = await client.Files.ListFolderAsync(path);

        if (result == null) return files;

        if (result.Entries == null || !result.Entries.Any()) return files;

        files.AddRange(result.Entries.Where(z => z.IsFile));

        var hasMoreFiles = result.HasMore;
        while (hasMoreFiles)
        {
            result = await client.Files.ListFolderAsync(path);
            files.AddRange(result.Entries.Where(z => z.IsFile));
            hasMoreFiles = result.HasMore;
        }

        return files;
    }

    public static async Task UploadFile(DropboxClient client, string localPath, string path)
    {
        var doChunkUpload = new FileInfo(localPath).Length > ChunkSize;
        if (doChunkUpload)
        {
            await ChunkUpload(client, localPath, path);
        }
        else
        {
            using var fs = new MemoryStream(await File.ReadAllBytesAsync(localPath));
            await client.Files.UploadAsync(path, WriteMode.Overwrite.Instance, body: fs);
        }
    }

    public static async Task DownloadFile(DropboxClient client, string path, string localPath)
    {
        await using var fs = File.Create(localPath);
        var response = await client.Files.DownloadAsync(path);
        var rs = await response.GetContentAsStreamAsync();
        await rs.CopyToAsync(fs);
    }

    private static async Task ChunkUpload(DropboxClient client, string localPath, string path)
    {
        using var stream = new MemoryStream(await File.ReadAllBytesAsync(localPath));
        var numChunks = (int)Math.Ceiling((double)stream.Length / ChunkSize);

        var buffer = new byte[ChunkSize];
        string? sessionId = null;

        for (var index = 0; index < numChunks; index++)
        {
            var byteRead = stream.Read(buffer, 0, ChunkSize);

            using var memStream = new MemoryStream(buffer, 0, byteRead);
            if (index == 0)
            {
                var result = await client.Files.UploadSessionStartAsync(body: memStream);
                sessionId = result.SessionId;
            }
            else
            {
                var cursor = new UploadSessionCursor(sessionId, (ulong)(ChunkSize * index));

                if (index == numChunks - 1)
                    await client.Files.UploadSessionFinishAsync(cursor, new CommitInfo(path, mode: WriteMode.Overwrite.Instance), body: memStream);
                else
                    await client.Files.UploadSessionAppendV2Async(cursor, body: memStream);
            }
        }
    }
}