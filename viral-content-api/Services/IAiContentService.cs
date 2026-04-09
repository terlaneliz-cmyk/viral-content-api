using ViralContentApi.DTOs;

namespace ViralContentApi.Services;

public interface IAiContentService
{
    Task<GenerateContentResponse> GenerateAsync(GenerateContentRequest request);
}