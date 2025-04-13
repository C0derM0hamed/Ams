using AmsApi.Config;
using AmsApi.Interfaces;
using AmsApi.Middleware;
using AmsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;

namespace AmsApi.Setup;

public static class AppConfiguration
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration config)
    {
        // إعداد JWT
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings!.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        // خدمات المشروع
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAttendeeService, AttendeeService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ISubjectDateService, SubjectDateService>();
        services.AddScoped<IConfigService, ConfigService>();

        // AutoMapper
        services.AddAutoMapper(typeof(Program));
        services.AddHttpContextAccessor();
    }

    public static void UseCustomMiddleware(this WebApplication app)
    {
        // الملفات الثابتة (صور وغيره)
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            RequestPath = ""
        });

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
