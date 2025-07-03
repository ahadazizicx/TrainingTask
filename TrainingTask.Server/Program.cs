using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Google.Cloud.Dialogflow.V2;
using TrainingTask.Server.Models;
using System.Text;
using TrainingTask.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddMongoDb();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TrainingTask API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// JWT Authentication setup
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Extract JWT from cookie if present, with logging
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger("JwtCookieAuth");
            var cookieToken = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(cookieToken))
            {
                context.Token = cookieToken;
            }
            else
            {
                logger?.LogWarning("No JWT found in cookie.");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var loggerFactory = context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggerFactory?.CreateLogger("JwtCookieAuth");
            logger?.LogError(context.Exception, "JWT authentication failed.");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<TrainingTask.Server.Services.IDialogflowService, TrainingTask.Server.Services.DialogflowService>();
builder.Services.AddScoped<TrainingTask.Server.Services.ChatWebSocketHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:4200")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); 

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();



app.MapControllers();

app.Map("/ws/chat", async context =>
{
    var handler = context.RequestServices.GetRequiredService<TrainingTask.Server.Services.ChatWebSocketHandler>();
    await handler.HandleAsync(context);
});

app.MapFallbackToFile("/index.html");

app.Run();