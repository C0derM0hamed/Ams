namespace AmsApi.Interfaces
{
    public interface IPythonClassifierClient
    {
        Task UploadClassifierAsync(IFormFile modelFile);
    }
}
