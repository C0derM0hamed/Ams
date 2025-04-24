using System.Net.Http.Headers;

namespace AmsApi.Services
{
    public class FaceRecognitionService
    {
        private readonly HttpClient _httpClient;

        public FaceRecognitionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:5000"); // بايثون سيرفر
        }

        public async Task<string?> ClassifyFaceAsync(IFormFile image)
        {
            using var content = new MultipartFormDataContent();
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var byteArray = ms.ToArray();

            var fileContent = new ByteArrayContent(byteArray);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.ContentType);
            content.Add(fileContent, "image", image.FileName);

            var response = await _httpClient.PostAsync("/classify", content);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<bool> UploadDatasetAsync()
        {
            var datasetRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dataset");
            if (!Directory.Exists(datasetRoot))
                return false;

            var directories = Directory.GetDirectories(datasetRoot);
            foreach (var dir in directories)
            {
                var studentName = Path.GetFileName(dir); // ex: attendee_1

                var imageFiles = Directory.GetFiles(dir);
                foreach (var imagePath in imageFiles)
                {
                    var fileStream = File.OpenRead(imagePath);
                    var fileName = Path.GetFileName(imagePath);

                    var form = new MultipartFormDataContent
                {
                    { new StringContent(studentName), "label" },
                    { new StreamContent(fileStream), "image", fileName }
                };

                    var response = await _httpClient.PostAsync("http://127.0.0.1:5000/upload_dataset", form);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"❌ Failed to upload {fileName}");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
