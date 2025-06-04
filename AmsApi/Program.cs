using AmsApi.Setup;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ✅ الترتيب مهم جداً

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles(); // لو فيه صور أو ملفات عامة

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomMiddleware(); // فيه Exception Middleware

app.MapControllers();

app.Run();
