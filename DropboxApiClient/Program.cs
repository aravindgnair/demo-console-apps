/*
 * Reference:
 * https://www.dropbox.com/developers/documentation/dotnet
 * https://github.com/dropbox/dropbox-sdk-dotnet/tree/main/dropbox-sdk-dotnet/Examples
 *
 */

using Dropbox.Api;
using Dropbox.Api.Common;
using Microsoft.Extensions.Configuration;

namespace DropboxApiClient;

public class Program
{
    private static string? _apiKey;
    private static string? _apiSecret;
    private static string? _userEmail;

    private static string? _accessToken;
    private static string? _refreshToken;
    private static string? _path;
    private static string[] _scopeList = [];

    private static async Task Main(string[] args)
    {
        var instance = new Program();
        try
        {
            await instance.Run(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task Run(bool isTeam)
    {
        InitConfiguration(isTeam);

        if (_accessToken == null)
        {
            var response = await new DropboxAuth(_apiKey!, _apiSecret!).AcquireAccessToken(_scopeList) ??
                           throw new Exception("Failed to get access token");
            _accessToken = response.AccessToken;
            _refreshToken = response.RefreshToken;
        }

        var dropboxClient = await GetDropboxClient(isTeam);
        var files = await DropboxService.ListFolder(dropboxClient, _path!);

        var fileList = files.ToList();

        Console.WriteLine("Listing files in folder {0}", _path);
        foreach (var file in fileList)
            Console.WriteLine(file.Name);

        /*

        var firstFile = fileList.First();
        Console.WriteLine("\r\nDownloading file {0}", firstFile.Name);
        await DropboxService.DownloadFile(dropboxClient, firstFile.PathDisplay,
            $"{AppDomain.CurrentDomain.BaseDirectory}/{firstFile.Name}");

        var fileToUpload = firstFile.Name;
        Console.WriteLine("\r\nUploading file {0}", fileToUpload);
        await DropboxService.UploadFile(dropboxClient, $"{AppDomain.CurrentDomain.BaseDirectory}/{fileToUpload}",
            $"{_path}/{new Random().NextInt64()}-{fileToUpload}");

        */
    }

    private static void InitConfiguration(bool isTeam)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

        _apiKey = config.GetRequiredSection("Dropbox:ApiKey").Get<string>();
        _apiSecret = config.GetRequiredSection("Dropbox:ApiSecret").Get<string>();
        _userEmail = config.GetRequiredSection("Dropbox:UserEmail").Get<string>();

        if (isTeam)
        {
            _accessToken = config.GetRequiredSection("Dropbox:TeamAccessToken").Get<string>();
            _refreshToken = config.GetRequiredSection("Dropbox:TeamRefreshToken").Get<string>();
            _path = config.GetRequiredSection("Dropbox:TeamDirectory").Get<string>();
            _scopeList = ["members.read", "team_data.member", "files.metadata.read", "files.content.read", "files.content.write", "account_info.read"];
        }
        else
        {
            _accessToken = config.GetRequiredSection("Dropbox:UserAccessToken").Get<string>();
            _refreshToken = config.GetRequiredSection("Dropbox:UserRefreshToken").Get<string>();
            _path = config.GetRequiredSection("Dropbox:UserDirectory").Get<string>();
            _scopeList = ["files.metadata.read", "files.content.read", "files.content.write", "account_info.read"];
        }

        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret) || string.IsNullOrEmpty(_userEmail) ||
            string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken) || string.IsNullOrEmpty(_path))
        {
            throw new Exception("Some configurations are missing required");
        }
    }

    private static async Task<DropboxClient> GetDropboxClient(bool isTeam)
    {
        var config = new DropboxClientConfig("demo_dropbox_api_client")
        {
            HttpClient = new HttpClient
            {
                // Specify request level timeout which decides maximum time that can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            }
        };

        if (!isTeam) return new DropboxClient(_accessToken, _refreshToken, _apiKey, _apiSecret, config);

        var teamClient = new DropboxTeamClient(_accessToken, _refreshToken, _apiKey, _apiSecret, config);
        var mlResult = await teamClient.Team.MembersListAsync();
        var member = mlResult.Members.SingleOrDefault(z => z.Profile.Email == _userEmail) ??
                     throw new Exception("Member not found in team");

        var userClient = teamClient.AsMember(member.Profile.TeamMemberId);
        var account = await userClient.Users.GetCurrentAccountAsync();
        userClient = userClient.WithPathRoot(new PathRoot.Root(account.RootInfo.RootNamespaceId));
        return userClient;
    }
}