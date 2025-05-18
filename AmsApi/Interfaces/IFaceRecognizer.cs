namespace AmsApi.Interfaces
{
    public interface IFaceRecognizer
    {
        Task<double[]> EmbedAsync(byte[] image);
        Task<Guid> ClassifyAsync(byte[] image);
    }

    public class FaceRecognizer : IFaceRecognizer
    {
        public Task<double[]> EmbedAsync(byte[] image)
        {
            // Implement your embedding logic here
            return Task.FromResult(new double[] { 0.1, 0.2, 0.3 }); // Sample embedding
        }

        public Task<Guid> ClassifyAsync(byte[] image)
        {
            // Implement your classification logic here
            return Task.FromResult(Guid.NewGuid()); // Sample classification
        }
    }

}
