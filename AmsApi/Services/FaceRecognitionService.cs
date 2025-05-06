using System.Net.Http.Headers;

namespace AmsApi.Services
{
    public class FaceRecognitionService
    {
        private readonly HttpClient _httpClient;
        private const string FaceApiBaseUrl = "http://localhost:5001"; // أو من config

        public FaceRecognitionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ClassifyAsync(Stream imageStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(imageStream), "image", fileName);

            var response = await _httpClient.PostAsync($"{FaceApiBaseUrl}/classify", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
