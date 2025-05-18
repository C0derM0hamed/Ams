using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmsApi.Helpers
{
    public class JwtHelper
    {
        private static readonly string SecretKey;
        private static string Issuer;
        private static string Audience;
        private static int ExpiryInMinutes;

        // Static constructor to initialize values from appsettings.json
        static JwtHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            // Read values from appsettings.json
            var jwtSettings = config.GetSection("JwtSettings");
            SecretKey = config.GetValue<string>("JwtSettings:SecretKey")!;
            Issuer = jwtSettings["Issuer"]!;
            Audience = jwtSettings["Audience"]!;
            ExpiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);
        }

        // التعديل هنا لإضافة الـ AdminId
        public static string GenerateToken(Guid userId, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // "sub" هو المستخدم
                new Claim("role", role), // "role" هو الدور
                new Claim("adminId", userId.ToString()) // "adminId" هو الـ AdminId
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(ExpiryInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // التعديل هنا لاستخراج الـ AdminId من التوكين
        public static ClaimsPrincipal? ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero // عشان تكون مدة الانتهاء دقيقة جدًا
                };

                var principal = handler.ValidateToken(token, parameters, out _);

                // استخراج الـ AdminId من التوكين
                var adminIdClaim = principal?.FindFirst("adminId")?.Value;
                if (adminIdClaim != null)
                {
                    // هنا ممكن ترجع الـ AdminId كـ Guid
                    var adminId = Guid.Parse(adminIdClaim);
                    // استخدام الـ adminId هنا إذا كنت بحاجة له
                }

                return principal;
            }
            catch
            {
                return null; // في حالة فشل التحقق
            }
        }
    }
}
