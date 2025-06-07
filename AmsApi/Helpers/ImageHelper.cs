namespace AmsApi.Helpers
{
    public static class ImageHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile file, string basePath, Guid attendeeId)
        {
            if (file == null || file.Length == 0)
                throw new Exception("No file uploaded.");

            var uploadDir = Path.Combine(basePath, "uploads", attendeeId.ToString());

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var imagePath = Path.Combine(uploadDir, "profile.png");

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // مسار نسبي مناسب للـ frontend
            return $"/uploads/{attendeeId}/profile.png";
        }
    }
}
