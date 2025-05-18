namespace AmsApi.Helpers
{
    public static class ImageHelper
    {
        public static async Task<byte[]> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("No file uploaded.");
            }

            // تحويل الصورة إلى byte[] بدلاً من string
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
