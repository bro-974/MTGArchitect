using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MTGArchitect.Data.Extensions;
using MTGArchitectServices.AuthApiService.Options;
using MTGArchitectServices.AuthApiService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend-dev", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var authConnectionString = builder.Configuration.GetConnectionString("authdb")
    ?? throw new InvalidOperationException("Connection string 'authdb' is missing.");

builder.Services.AddAuthData(authConnectionString);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) || string.IsNullOrWhiteSpace(jwtOptions.Audience) || string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new InvalidOperationException("JWT configuration is incomplete.");

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

await app.SeedDefaultAuthUserAsync();

app.UseExceptionHandler();
app.UseCors("frontend-dev");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapDefaultEndpoints();

app.Run();
