using FluentValidation;
using MeetingSummary.Core.Interfaces;
using MeetingSummary.Services.Implementations;
using MeetingSummary.TranscriptionWorker.BackgroundServices;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets(typeof(Program).Assembly, optional: true);

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var cosmosConnectionString = configuration["CosmosDb:ConnectionString"] 
        ?? throw new InvalidOperationException("CosmosDb:ConnectionString is required");

    CosmosClientOptions options = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Gateway,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true
        }
    };

    return new CosmosClient(cosmosConnectionString, options);
});

builder.Services.AddScoped<IMeetingChunkProcessor, MeetingChunkProcessor>();
builder.Services.AddScoped<ITranscriptRepository, CosmosDbTranscriptRepository>();
builder.Services.AddScoped<ITranscriptionService, StubTranscriptionService>();
builder.Services.AddValidatorsFromAssemblyContaining<MeetingChunkProcessor>();
builder.Services.AddSingleton<TranscriptionWorkerService>();
builder.Services.AddHostedService<TranscriptionWorkerService>();

var host = builder.Build();
await host.RunAsync();
