using Azure.AI.OpenAI;
using MeetingSummary.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

namespace MeetingSummary.Services.Implementations;

public class AzureWhisperTranscriptionService : ITranscriptionService
{
    private readonly AzureOpenAIClient _openAiClient;
    private readonly string _deploymentName;

    public AzureWhisperTranscriptionService(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("AzureOpenAI:Endpoint");
        var apiKey = configuration["AzureOpenAI:Key"] ?? throw new ArgumentNullException("AzureOpenAI:Key");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("AzureOpenAI:DeploymentName");

        _deploymentName = deploymentName;
        _openAiClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
    }

    public async Task<string> TranscribeAudio(string blobUrl, string language)
    {
        var audioStream = await GetStreamFromBlobUrlAsync(blobUrl);
        var response = await TranscribeFromStreamAsync(audioStream, "AudioFile.m4a");
        return response;
    }

    private async Task<string> TranscribeFromStreamAsync(Stream audioStream, string filename)
    {
        var response = await _openAiClient.GetAudioClient(_deploymentName).TranscribeAudioAsync(audioStream, filename);
        var transcription = response.Value;
        return transcription.Text;
    }

    private async Task<Stream> GetStreamFromBlobUrlAsync(string blobUrl)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(blobUrl);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}

