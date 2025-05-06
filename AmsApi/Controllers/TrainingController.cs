using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.IO;

namespace AmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private readonly FaceDatasetUploaderService _service;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;
        public TrainingController(FaceDatasetUploaderService service, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        [HttpPost("sync-to-python")]
        public async Task<IActionResult> SyncAllToPython()
        {
            // رفع كل الصور إلى سيرفر بايثون
            await _service.UploadAllTrainingImagesToPythonAsync();
            return Ok("All images synced to Python service successfully.");
        }


        // Endpoint لبدء التدريب
        [HttpPost("train-model")]
        public async Task<IActionResult> TrainModel()
        {
            var pythonEndpoint = "http://127.0.0.1:5000/train"; // عنوان سيرفر البايثون

            // رفع الصور إلى سيرفر البايثون أولًا
            var client = _httpClientFactory.CreateClient();

            // استخدام المسار الصحيح للصور داخل WebRootPath
            var basePath = Path.Combine(_env.WebRootPath, "dataset"); // تأكد من أنك تستخدم المسار الصحيح
            var imageFiles = Directory.GetFiles(basePath); // مسار الصور على السيرفر
            foreach (var imageFile in imageFiles)
            {
                var imageContent = new MultipartFormDataContent();
                var image = new ByteArrayContent(System.IO.File.ReadAllBytes(imageFile));
                image.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                imageContent.Add(image, "image", Path.GetFileName(imageFile));
                var response = await client.PostAsync("http://127.0.0.1:5000/upload-image", imageContent);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, $"Failed to upload image {imageFile}");
                }
            }

            // بعد رفع الصور بنجاح، بدء التدريب
            var responseTrain = await client.PostAsync(pythonEndpoint, null);
            if (!responseTrain.IsSuccessStatusCode)
            {
                return StatusCode(500, "Failed to start model training.");
            }

            return Ok("Model training started successfully.");
        }
    }
}
