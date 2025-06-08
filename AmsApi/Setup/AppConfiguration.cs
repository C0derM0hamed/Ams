using AmsApi.Config;
using AmsApi.Interfaces;
using AmsApi.Middleware;
using AmsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace AmsApi.Setup;

public static class AppConfiguration
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new { message = "Unauthorized" });
                    return context.Response.WriteAsync(result);
                }
            };
        });



        services.AddScoped<IAttendeeService, AttendeeService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IUserService, UserService>();
        services.AddHttpClient();
        services.AddHttpClient<FaceRecognitionService>();
        services.AddScoped<IJwtHelper, JwtHelper>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddHttpContextAccessor();
        services.AddScoped<ISettingsService, SettingsService>();

        services.AddIdentityCore<AppUser>(options => { })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AmsDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
    }

    public static void UseCustomMiddleware(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            RequestPath = ""
        });

        app.UseMiddleware<ExceptionMiddleware>();
    }
}
