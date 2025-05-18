namespace AmsApi.Services
{
    // PythonClassifierClient.cs
    public class PythonClassifierClient : IPythonClassifierClient
    {
        private readonly HttpClient _http;

        public PythonClassifierClient(HttpClient http) => _http = http;

        public async Task UploadClassifierAsync(IFormFile modelFile)
        {
            using var content = new MultipartFormDataContent();
            using var stream = modelFile.OpenReadStream();
            content.Add(new StreamContent(stream), "model", modelFile.FileName);

            var res = await _http.PostAsync("/upload_classifier", content);
            res.EnsureSuccessStatusCode();
        }
    }

}
