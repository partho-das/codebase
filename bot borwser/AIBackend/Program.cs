using AIBackend.AIClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddJsonFile("appsettings.json", optional:true, reloadOnChange: true).AddEnvironmentVariables();



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

var provider  = builder.Configuration["AI:Provider"]?.ToLower() ?? "huggingface";
builder.Services.AddSingleton<IAiService, HuggingFaceChatClient>();

var app = builder.Build();



// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
