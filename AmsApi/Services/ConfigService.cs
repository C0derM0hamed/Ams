using AmsApi.Interfaces;

namespace AmsApi.Services;

public class ConfigService : IConfigService
{
    private static bool _faceRecognitionEnabled = false;
    private readonly FaceRecognitionService _faceRecognition;
    public ConfigService(FaceRecognitionService faceRecognition)
    {
        _faceRecognition = faceRecognition;
    }
    public Task<bool> TrainClassifierAsync()
    {
        // هنا ممكن تنادي Python script أو AI model
        Console.WriteLine("Training classifier...");
        return Task.FromResult(true);
    }

    public Task<bool> ToggleFaceRecognitionAsync(bool enabled)
    {
        _faceRecognitionEnabled = enabled;
        return Task.FromResult(true);
    }

    public bool IsFaceRecognitionEnabled()
    {
        return _faceRecognitionEnabled;
    }
    public async Task<bool> UploadDatasetAsync()
    {
        return await _faceRecognition.UploadDatasetAsync();
    }

}