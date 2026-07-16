using Backend_Mboagreen.Configurations;
using Backend_Mboagreen.Data;
using Backend_Mboagreen.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Controllers
builder.Services.AddControllers();


// CamerPay
builder.Services.AddHttpClient<CamerPayService>();


// Service Email
builder.Services.AddScoped<IEmailService, EmailService>();


// Service Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

builder.Services.AddScoped<
    ICloudinaryService,
    CloudinaryService>();


// Base de données
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )));


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://unable-aunt.surge.sh"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});


// JWT
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new Exception(
        "JWT Key manquant ou trop court (minimum 32 caractères).");
}


builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,

            ValidIssuer =
            builder.Configuration["Jwt:Issuer"]
            ?? "MboaGreen",

            ValidateAudience = true,

            ValidAudience =
            builder.Configuration["Jwt:Audience"]
            ?? "MboaGreenUsers",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,

            IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };


    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(

                System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        message =
                        "Token invalide ou expiré"
                    })

                );
        }
    };

});


builder.Services.AddAuthorization();


// Logging
builder.Services.AddLogging();


var app = builder.Build();


// CORS
app.UseCors("AllowReactApp");


// HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// Headers de sécurité
app.Use(async (context, next) =>
{
    context.Response.Headers.Append(
        "X-Content-Type-Options",
        "nosniff");

    context.Response.Headers.Append(
        "X-Frame-Options",
        "DENY");

    context.Response.Headers.Append(
        "X-XSS-Protection",
        "1; mode=block");

    context.Response.Headers.Append(
        "Strict-Transport-Security",
        "max-age=31536000; includeSubDomains");

    await next();
});


// Authentification
app.UseAuthentication();

app.UseAuthorization();


// Controllers
app.MapControllers();


app.Run();