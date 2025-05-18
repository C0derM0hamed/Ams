namespace AmsApi.Services
{
    public class FaceRecModeService
    {
        // Default to Embed
        public FaceRecMode CurrentMode { get; private set; } = FaceRecMode.Embed;
        public void SetMode(FaceRecMode mode) => CurrentMode = mode;
    }
}
