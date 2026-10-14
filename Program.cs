using Backend_Mboagreen.Configurations;
using Backend_Mboagreen.Data;
using Backend_Mboagreen.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);



// =======================
// CONTROLLERS
// =======================

builder.Services.AddControllers();



// =======================
// SERVICES
// =======================

builder.Services.AddHttpClient<CamerPayService>();
builder.Services.AddSingleton<PaymentSimulator>();

builder.Services.AddScoped<IEmailService, EmailService>();


builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);


builder.Services.AddScoped<
    ICloudinaryService,
    CloudinaryService
>();




// =======================
// BASE DE DONNEES
// =======================

var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(postgresConnection))
{
    throw new InvalidOperationException(
        "Configurez ConnectionStrings__DefaultConnection avec la chaîne PostgreSQL."
    );
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresConnection)
);




// =======================
// CORS
// =======================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
        .WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "http://localhost:5175"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});




// =======================
// JWT
// =======================

var jwtKey = builder.Configuration["Jwt:Key"];


if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new Exception(
        "JWT Key manquant ou trop court."
    );
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
        builder.Configuration["Jwt:Issuer"],


        ValidateAudience = true,

        ValidAudience =
        builder.Configuration["Jwt:Audience"],


        ValidateLifetime = true,


        ValidateIssuerSigningKey = true,


        IssuerSigningKey =
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),


        ClockSkew = TimeSpan.Zero
    };

});



builder.Services.AddAuthorization();

builder.Services.AddLogging();




// =======================
// BUILD
// =======================

var app = builder.Build();




// =======================
// CORS
// =======================

app.UseCors("AllowReactApp");




// =======================
// HTTPS
// =======================

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}





// =======================
// HEADERS SECURITE
// =======================

app.Use(async (context, next) =>
{

    context.Response.Headers.Append(
        "X-Content-Type-Options",
        "nosniff"
    );


    context.Response.Headers.Append(
        "X-Frame-Options",
        "DENY"
    );


    await next();

});





// =======================
// DOSSIER UPLOADS
// =======================


var wwwrootPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "wwwroot"
);


var uploadsPath = Path.Combine(
    wwwrootPath,
    "uploads"
);



if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}



if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}




// =======================
// FICHIERS STATIQUES
// =======================

app.UseStaticFiles(
    new StaticFileOptions
    {

        FileProvider =
        new PhysicalFileProvider(
            wwwrootPath
        ),

        RequestPath = ""

    }
);





// =======================
// AUTH
// =======================

app.UseAuthentication();

app.UseAuthorization();




// =======================
// ROUTES API
// =======================

app.MapControllers();




// =======================
// START
// =======================

app.Run();