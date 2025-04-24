namespace AmsApi.Interfaces
{
    public interface IConfigService
    {
        Task<bool> TrainClassifierAsync();
        Task<bool> ToggleFaceRecognitionAsync(bool enabled);
        bool IsFaceRecognitionEnabled();
        public Task<bool> UploadDatasetAsync();
    }

}
