using System.Net.Http.Headers;

namespace AmsApi.Services
{
    public class FaceRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _pythonUrl;

        public FaceRecognitionService(HttpClient httpClient,IConfiguration config)
        {
            _httpClient = httpClient;
            _pythonUrl = config["PythonFaceRec:BaseUrl"]!;

        }

        public async Task<string> ClassifyAsync(Stream imageStream, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(imageStream), "image", fileName);

            var response = await _httpClient.PostAsync($"{_pythonUrl}/classify", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
