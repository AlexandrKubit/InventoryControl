using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net;
using System.Net.Http.Json;

namespace UI.Services;

public static class HttpService
{
    public static string baseUrl = "http://localhost:5000";

    public static async Task<TResponse> GetDataAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var httpClient = new HttpClient(new CookieHandler());
        var result = await httpClient.PostAsJsonAsync($"{baseUrl}{url}", request);

        if(result.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await result.Content.ReadFromJsonAsync<Msg>();
            Components.Toast.AddMessage(message.Message);
            return default;
        }
        if (result.StatusCode == HttpStatusCode.Conflict)
        {
            var message = await result.Content.ReadFromJsonAsync<Msg>();
            Components.Toast.AddMessage(message.Message);
            return default;
        }
        if (result.StatusCode == HttpStatusCode.InternalServerError)
        {
            Components.Toast.AddMessage("Извините, произошла неожиданная ошибка");
            return default;
        }

        var response = await result.Content.ReadFromJsonAsync<TResponse>();
        return response;
    }
}

public class CookieHandler : DelegatingHandler
{
    public CookieHandler()
    {
        InnerHandler = new HttpClientHandler();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}

public class Msg
{
    public string Message { get; set; }
}