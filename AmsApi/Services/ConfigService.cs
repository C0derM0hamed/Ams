using System.Net.Http.Headers;
using System.Text.Json;

public class ConfigService : IConfigService
{
    private readonly HttpClient _http;

    public ConfigService(HttpClient http)
    {
        _http = http;
    }

    public async Task SetFaceRecModeAsync(FaceRecMode mode, string jwtToken)
    {
        var modeStr = mode.ToString(); // "Embed" or "Classify"
        using var req = new HttpRequestMessage(HttpMethod.Put, $"/config/face_recognition?mode={modeStr}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UploadClassifierAsync(IFormFile classifierFile, string jwtToken)
    {
        using var content = new MultipartFormDataContent();
        using var stream = classifierFile.OpenReadStream();
        content.Add(new StreamContent(stream)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") }
        }, "classifier", classifierFile.FileName);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/config/classifier")
        {
            Content = content
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }
}


