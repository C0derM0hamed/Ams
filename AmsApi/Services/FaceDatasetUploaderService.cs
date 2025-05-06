using System.Net.Http.Headers;
namespace AmsApi.Services
{
    public class FaceDatasetUploaderService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public FaceDatasetUploaderService(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public async Task UploadAllTrainingImagesToPythonAsync()
        {
            // تحديد المسار الصحيح للصور داخل WebRootPath
            var basePath = Path.Combine(_env.WebRootPath, "dataset");  // مسار wwwroot/dataset
            var client = _httpClientFactory.CreateClient();
            var pythonEndpoint = "http://127.0.0.1:5000/upload-image";  // تأكد من المسار الصحيح في سيرفر بايثون

            // التأكد من أن المجلد موجود
            if (!Directory.Exists(basePath))
            {
                throw new DirectoryNotFoundException("Dataset directory not found.");
            }

            foreach (var studentDir in Directory.GetDirectories(basePath))
            {
                var label = Path.GetFileName(studentDir); // attendee_1 مثلا

                foreach (var imagePath in Directory.GetFiles(studentDir))
                {
                    try
                    {
                        // إنشاء نموذج MultipartFormDataContent
                        using var form = new MultipartFormDataContent();
                        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                        // إضافة الصورة إلى الطلب
                        form.Add(fileContent, "image", Path.GetFileName(imagePath));
                        form.Add(new StringContent(label), "label");  // إضافة الـ label (اسم الشخص)

                        // إرسال الطلب إلى سيرفر بايثون
                        var response = await client.PostAsync(pythonEndpoint, form);

                        // التأكد من نجاح الطلب
                        if (!response.IsSuccessStatusCode)
                        {
                            // طباعة الخطأ في حالة فشل الرفع
                            Console.WriteLine($"Failed to upload image {imagePath} with status code {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // التعامل مع الأخطاء
                    }
                }
            }
        }
    }
}