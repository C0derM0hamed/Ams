using AmsApi.Interfaces;
using System.Net.Http.Headers;

namespace AmsApi.Services
{
    public class FaceRecognitionService
    {
        private readonly HttpClient _httpClient;
        
        private readonly ISettingsService _settingsService;

        public FaceRecognitionService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
        }

        public async Task<string> ClassifyAsync(Stream imageStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(imageStream), "image", fileName);
            var baseUrl = await _settingsService.GetValueAsync("PythonFaceRec.BaseUrl");
            var response = await _httpClient.PostAsync($"{baseUrl}/classify", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
