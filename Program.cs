using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Taxi_API.Data;
using Taxi_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Bind to localhost on port 5000 so the API and Swagger run on http://localhost:5000
//builder.WebHost.UseUrls("http://localhost:5000");
builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=taxi.db"));

builder.Services.AddScoped<IStorageService, LocalStorageService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IImageComparisonService, OpenCvImageComparisonService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "very_secret_key_please_change";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "TaxiApi";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Haarcascade file should be placed into application base directory or bundled in deployment: "haarcascade_frontalface_default.xml"

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger UI and configure it to use localhost:5000
app.UseSwagger();
var swaggerHost = "localhost:5000";
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"http://{swaggerHost}/swagger/v1/swagger.json", "Taxi API V1");
    c.RoutePrefix = "swagger"; // serve at /swagger
});

// redirect root to swagger UI on localhost:5000
app.MapGet("/", () => Results.Redirect("http://localhost:5000/swagger/index.html"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();