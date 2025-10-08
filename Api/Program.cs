using Api.Mapper;
using Bycript;
using Database.Data;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Validation;
using AppAny.HotChocolate.FluentValidation;
using Minio;
using Bucket;
using DataAnnotatedModelValidations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Leer variables de entorno
var jwtSecret = builder.Configuration["JWT__Secret"] ?? Environment.GetEnvironmentVariable("JWT__Secret") ?? "Micontraseñasupersecreta";
var jwtIssuer = builder.Configuration["JWT__Issuer"] ?? Environment.GetEnvironmentVariable("JWT__Issuer") ?? "https://nelson.com";
var jwtAudience = builder.Configuration["JWT__Audience"] ?? Environment.GetEnvironmentVariable("JWT__Audience") ?? "https://nelson.com";

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Server=interchange.proxy.rlwy.net;Port=49952;Database=railway;Uid=root;Pwd=aRcZJTlisNyoJXrKRLmaQceAwtQgGfiw;";

var minioEndpoint = builder.Configuration["MinIO__Endpoint"] ?? Environment.GetEnvironmentVariable("MinIO__Endpoint") ?? "localhost";
var minioPort = int.Parse(builder.Configuration["MinIO__Port"] ?? Environment.GetEnvironmentVariable("MinIO__Port") ?? "9000");
var minioAccessKey = builder.Configuration["MinIO__AccessKey"] ?? Environment.GetEnvironmentVariable("MinIO__AccessKey") ?? "minioadmin";
var minioSecretKey = builder.Configuration["MinIO__SecretKey"] ?? Environment.GetEnvironmentVariable("MinIO__SecretKey") ?? "minioadmin";
var minioUseSSL = bool.Parse(builder.Configuration["MinIO__UseSSL"] ?? Environment.GetEnvironmentVariable("MinIO__UseSSL") ?? "false");

var contra = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.AddGraphQL().AddTypes()
.AddProjections()
.AddFiltering()
.AddSorting()
.AddPagingArguments()
.AddFluentValidation()
.AddType<UploadType>()
.AddDataAnnotationsValidator();

builder.Services.AddDbContext<DatabseContext>(opt => opt.UseMySQL(connectionString));

builder.Services
 .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = contra
                    };
            });

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(minioEndpoint, minioPort)
    .WithCredentials(minioAccessKey, minioSecretKey)
    .WithSSL(minioUseSSL));

builder.Services.AddScoped<IMinioService>(provider =>
{
    var minioClient = provider.GetRequiredService<IMinioClient>();
    return new MinioService(minioClient, minioEndpoint, minioPort, minioUseSSL);
});
builder.Services.AddScoped<Mapa>();

builder.Services.AddScoped<IBCryptService, BCryptService>();

// Configurar CORS para permitir acceso desde app móvil
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Usar CORS
app.UseCors("AllowMobileApp");


app.MapGraphQL();

app.RunWithGraphQLCommands(args);
