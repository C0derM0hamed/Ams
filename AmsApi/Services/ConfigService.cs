using AmsApi.DTOs;
using AmsApi.Interfaces;

namespace AmsApi.Services
{
    public class ConfigService : IConfigService
    {
        public Task<string> TrainClassifierAsync(ClassifierConfigDto dto)
        {
            // mock logic
            var message = $"Classifier trained. Info: {dto.Info ?? "default"}";
            return Task.FromResult(message);
        }

        public Task<string> UpdateFaceModelAsync(FaceRecognitionDto dto)
        {
            // mock logic
            var message = $"Face model '{dto.ModelName}' updated with threshold {dto.Threshold}";
            return Task.FromResult(message);
        }
    }
}
