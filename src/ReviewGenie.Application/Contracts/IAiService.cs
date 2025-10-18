using ReviewGenie.Application.Dto;

namespace ReviewGenie.Application.Contracts;

public interface IAiService
{
    Task<string> AnalyzeSentimentAsync(string reviewText);
    Task<string> GenerateResponseAsync(GenerateResponseDto request);
    Task<(string sentiment, string response)> AnalyzeAndGenerateResponseAsync(GenerateResponseDto request);
}
