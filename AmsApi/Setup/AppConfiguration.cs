using AmsApi.Config;
using AmsApi.Interfaces;
using AmsApi.Middleware;
using AmsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AmsApi.Setup;

public static class AppConfiguration
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
       
        services.AddSingleton(jwtSettings.SecretKey);
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
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });


        // خدمات المشروع

        //  • register the mode holder
        services.AddSingleton<FaceRecModeService>();

        //  • register our Python‐client against the configured base URL
        var pyBase = config["PythonFaceRec:BaseUrl"]!;
        services.AddHttpClient<IPythonClassifierClient, PythonClassifierClient>(client =>
        {
            client.BaseAddress = new Uri(pyBase);
        });

        services.AddScoped<IAttendeeService, AttendeeService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        
        services.AddScoped<IConfigService, ConfigService>();

        services.AddScoped<FaceDatasetUploaderService>();
        services.AddHttpClient();
        services.AddHttpClient<FaceRecognitionService>();

        services.AddSingleton<JwtHelper>();
        services.AddIdentity<AppUser, IdentityRole>()
         .AddEntityFrameworkStores<AmsDbContext>()
         .AddDefaultTokenProviders();

        // AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddHttpContextAccessor();

        services.AddScoped<IFaceRecognizer, Services.FaceRecognizer> ();

    }

    public static void UseCustomMiddleware(this WebApplication app)
    {


        // ملفات الصور المخزنة في wwwroot/dataset
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
