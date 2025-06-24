using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Google.Cloud.Dialogflow.V2;
using TrainingTask.Server.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication setup
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TrainingTask.Server";

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

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();

app.MapControllers();

app.Map("/ws/chat", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[1024 * 4];
    while (webSocket.State == WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            break;
        }
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var chatRequest = JsonSerializer.Deserialize<ChatRequest>(message);
        // Use static config for demo
        var config = TrainingTask.Server.Controllers.ChatController.GetStaticConfig();
        var credentialsJson = config.CredentialsJson;
        var languageCode = config.LanguageCode;
        var projectId = "";
        if (!string.IsNullOrEmpty(credentialsJson))
        {
            using var doc = JsonDocument.Parse(credentialsJson);
            if (doc.RootElement.TryGetProperty("project_id", out var pid))
                projectId = pid.GetString();
        }
        string fulfillmentText = "", intentName = "";
        if (!string.IsNullOrEmpty(credentialsJson) && !string.IsNullOrEmpty(projectId))
        {
            var builder = new SessionsClientBuilder { JsonCredentials = credentialsJson };
            var sessionsClient = await builder.BuildAsync();
            var sessionName = SessionName.FromProjectSession(projectId, chatRequest.SessionId);
            var queryInput = new QueryInput
            {
                Text = new TextInput { Text = chatRequest.Message, LanguageCode = languageCode }
            };
            var detectIntentRequest = new DetectIntentRequest
            {
                SessionAsSessionName = sessionName,
                QueryInput = queryInput
            };
            var response = await sessionsClient.DetectIntentAsync(detectIntentRequest);
            fulfillmentText = response.QueryResult.FulfillmentText;
            intentName = response.QueryResult.Intent.DisplayName;
        }
        var reply = JsonSerializer.Serialize(new { fulfillmentText, intentName });
        var replyBytes = Encoding.UTF8.GetBytes(reply);
        await webSocket.SendAsync(new ArraySegment<byte>(replyBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
});

app.MapFallbackToFile("/index.html");

app.Run();