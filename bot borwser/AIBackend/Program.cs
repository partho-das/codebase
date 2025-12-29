using AIBackend.Ai.Tools;
using AIBackend.AIClient;
using AIBackend.Config;
using AIBackend.Interfaces;
using AIBackend.MCP;
using AIBackend.Stores;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Configuration.AddJsonFile("appsettings.json", optional:true, reloadOnChange: true).AddEnvironmentVariables();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();


var provider  = builder.Configuration["AI:Provider"]?.ToLower() ?? "huggingface";
if (provider == "huggingface")
    builder.Services.AddSingleton<IAiService, HuggingFaceChatClient>();
else if (provider == "ollama")
    builder.Services.AddSingleton<IAiService, CustomOllamaChatClient>(); // Use custom Ollama client
else if (provider == "ollama-semantic")
    builder.Services.AddSingleton<IAiService, SemanticKernelOllamaChatClient>(); // Semantic Kernel
else if (provider == "ollama-langchain")
    builder.Services.AddSingleton<IAiService, OllamaChatClient>(); // LangChain implementation
else
    builder.Services.AddSingleton<IAiService, CustomOllamaChatClient>(); // Default to custom Ollama

builder.Services.AddSingleton<IAiSessionStore, AiSessionStore>();
builder.Services.AddSingleton<IToolExecutor, McpToolExecutor>();

builder.Services.AddSingleton<ToolRegistry>(sp =>
    {
        var registry = new ToolRegistry();
         registry.Register(new GetWeatherTool());
        registry.Register(new CalculateSumTool());
        return registry;
    });

var app = builder.Build();



// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization(); 

app.MapControllers();

app.Run();
