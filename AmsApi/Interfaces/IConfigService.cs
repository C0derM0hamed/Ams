namespace AmsApi.Interfaces
{
    public interface IConfigService
    {
        Task SetFaceRecModeAsync(FaceRecMode mode, string jwtToken);
        Task UploadClassifierAsync(IFormFile classifierFile, string jwtToken);
    }
    public enum FaceRecMode
    {
        Embed,
        Classify
    }

}
