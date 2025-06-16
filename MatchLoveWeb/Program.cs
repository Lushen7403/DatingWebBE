using MatchLoveWeb.Services.Interfaces;
using MatchLoveWeb.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MatchLoveWeb.Mapping;
using MatchLoveWeb.Models;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.SignaIR;
using CloudinaryDotNet;
using Microsoft.AspNetCore.SignalR;
using static MatchLoveWeb.Controllers.PaymentController;
using Microsoft.ML.OnnxRuntime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatingWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])
            )
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                // Nếu request đến /chatHub và có access_token trong query string
                var token = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) &&
                (path.StartsWithSegments("/chatHub", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/notificationhub", StringComparison.OrdinalIgnoreCase)))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddSignalR();

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddScoped<PhotoService>();


builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(provider =>
{
    var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
    var acc = new CloudinaryDotNet.Account(config.CloudName, config.ApiKey, config.ApiSecret);
    return new CloudinaryDotNet.Cloudinary(acc);
});

builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

builder.Services.Configure<VnpayOptions>(builder.Configuration.GetSection("Vnpay"));

builder.Services.Configure<PaymentSettings>(
  builder.Configuration.GetSection("PaymentSettings"));

builder.Services.AddTransient<IVnpayService, VnpayService>();

builder.Services.AddHttpClient<TokenClassificationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000"); // Flask server đang chạy tại đây
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<ChatHub>("/chathub");

app.MapHub<NotificationHub>("/notificationhub");

app.MapControllers();

app.Run();
