using System.Net.Http.Headers;
namespace AmsApi.Services
{
    public class FaceDatasetUploaderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public FaceDatasetUploaderService(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public async Task UploadAllTrainingImagesToPythonAsync()
        {
            var basePath = Path.Combine(_env.WebRootPath, "dataset");
            var client = _httpClientFactory.CreateClient();
            var pythonEndpoint = "http://127.0.0.1:5000/upload_dataset";

            foreach (var studentDir in Directory.GetDirectories(basePath))
            {
                var label = Path.GetFileName(studentDir); // attendee_1 مثلا

                foreach (var imagePath in Directory.GetFiles(studentDir))
                {
                    using var form = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                    form.Add(fileContent, "image", Path.GetFileName(imagePath));
                    form.Add(new StringContent(label), "label");

                    var response = await client.PostAsync(pythonEndpoint, form);
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }
}
