using System.Diagnostics;
using System.Net;
using Dropbox.Api;

namespace DropboxApiClient;

public class DropboxAuth
{
    private const string RedirectPage = "index.html";

    public DropboxAuth(string apiKey, string apiSecret, string? loopbackHost = null)
    {
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        LoopbackHost = loopbackHost ?? "http://127.0.0.1:52475/";

        RedirectUri = new Uri(LoopbackHost + "authorize");
        JsRedirectUri = new Uri(LoopbackHost + "token");
        IncludeGrantedScopes = IncludeGrantedScopes.None;
    }

    public string ApiKey { get; }

    public string ApiSecret { get; }
    
    public string LoopbackHost { get; }
    
    public Uri RedirectUri { get; }
    
    public Uri JsRedirectUri { get; }

    public string[] ScopeList { get; set; } = [];

    public IncludeGrantedScopes IncludeGrantedScopes { get; set; }

    public async Task<OAuth2Response?> AcquireAccessToken(string[] scopeList)
    {
        var state = Guid.NewGuid().ToString("N");

        var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, ApiKey,
            RedirectUri, state, tokenAccessType: TokenAccessType.Offline, scopeList: scopeList,
            includeGrantedScopes: IncludeGrantedScopes);

        var http = new HttpListener();
        http.Prefixes.Add(LoopbackHost);
        http.Start();
        Process.Start(new ProcessStartInfo(authorizeUri.ToString()) { UseShellExecute = true });

        // Handle OAuth redirect and send URL fragment to local server using JS.
        await HandleOAuth2Redirect(http);

        // Handle redirect from JS and process OAuth response.
        var redirectUri = await HandleJsRedirect(http);

        var tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(redirectUri, ApiKey,
            ApiSecret, RedirectUri.ToString(), state);

        http.Stop();

        return tokenResult;
    }

    private async Task HandleOAuth2Redirect(HttpListener http)
    {
        var context = await http.GetContextAsync();

        // We only care about request to RedirectUri endpoint.
        while (context.Request.Url?.AbsolutePath != RedirectUri.AbsolutePath) context = await http.GetContextAsync();

        context.Response.ContentType = "text/html";

        // Respond with a page which runs JS and sends URL fragment as query string
        // to TokenRedirectUri.
        await using (var file = File.OpenRead(RedirectPage))
        {
            await file.CopyToAsync(context.Response.OutputStream);
        }

        context.Response.OutputStream.Close();
    }

    private async Task<Uri> HandleJsRedirect(HttpListener http)
    {
        var context = await http.GetContextAsync();

        // We only care about request to TokenRedirectUri endpoint.
        while (context.Request.Url?.AbsolutePath != JsRedirectUri.AbsolutePath) context = await http.GetContextAsync();

        var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]!);

        return redirectUri;
    }
}