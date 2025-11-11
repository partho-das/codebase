using AIBackend.AIClient;
using AIBackend.Config;
using AIBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Configuration.AddJsonFile("appsettings.json", optional:true, reloadOnChange: true).AddEnvironmentVariables();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient();

var provider  = builder.Configuration["AI:Provider"]?.ToLower() ?? "huggingface";
builder.Services.AddSingleton<IAiService, HuggingFaceChatClient>();
builder.Services.AddSingleton<CentrifugoService>();

var app = builder.Build();



// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization(); 

app.MapControllers();

app.Run();
