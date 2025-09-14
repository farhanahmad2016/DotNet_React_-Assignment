// Exam System API - Entry Point
// Configures services, authentication, CORS, and middleware pipeline

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using ExamSystem.API.Data;
using ExamSystem.API.Services;
using ExamSystem.API.Validators;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure API services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();

// Enhanced Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Exam System API",
        Version = "v1",
        Description = "A comprehensive exam management system with role-based access control",
        Contact = new OpenApiContact
        {
            Name = "Exam System Team",
            Email = "support@examsystem.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Entity Framework with in-memory database
builder.Services.AddDbContext<ExamDbContext>(options =>
    options.UseInMemoryDatabase("ExamSystemDb"));

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExamService, ExamService>();

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

// Validate JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing. Please ensure Jwt:Key, Jwt:Issuer, and Jwt:Audience are configured.");
}

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://192.168.1.3:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for JWT cookies
    });
});

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exam System API v1");
        c.RoutePrefix = "swagger"; // Access at /swagger
        c.DocumentTitle = "Exam System API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
    });
}

// Middleware order is important!
app.UseCors("AllowReactApp"); // Must be before authentication
app.UseAuthentication(); // Must be before authorization
app.UseAuthorization();
app.MapControllers();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ExamDbContext>();
    context.Database.EnsureCreated(); // Creates DB and runs seed data
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }