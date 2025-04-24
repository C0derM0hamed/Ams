using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace AmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private readonly FaceDatasetUploaderService _service;
        private readonly IHttpClientFactory _httpClientFactory;
        public TrainingController(FaceDatasetUploaderService service,IHttpClientFactory httpClientFactory)
        {
            _service = service;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("sync-to-python")]
        public async Task<IActionResult> SyncAllToPython()
        {
            await _service.UploadAllTrainingImagesToPythonAsync();
            return Ok("All images synced to Python service successfully.");
        }
        // Endpoint لبدء التدريب
        [HttpPost("train-model")]
        public async Task<IActionResult> TrainModel()
        {
            var pythonEndpoint = "http://127.0.0.1:5000/train"; // عنوان سيرفر البايثون

            using var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(pythonEndpoint, null);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(500, "Failed to start model training.");
            }

            return Ok("Model training started successfully.");
        }

    }
}