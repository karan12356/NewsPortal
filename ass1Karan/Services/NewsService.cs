using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ass1Karan.Models;

namespace ass1Karan.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NewsService> _logger;

        public NewsService(HttpClient httpClient, IConfiguration config, ILogger<NewsService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<List<NewsArticle>> GetTopHeadlinesAsync()
        {
            try
            {
                var apiUrl = _config["NewsApi:BaseUrl"];
                var apiKey = _config["NewsApi:ApiKey"];
                var country = _config["NewsApi:Country"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("News API configuration is missing or incomplete.");
                    return new List<NewsArticle>();
                }

                string requestUrl = $"{apiUrl}?country={country}&apiKey={apiKey}";
                _logger.LogInformation($"Fetching news from: {requestUrl}");

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch news. Status code: {response.StatusCode}");
                    return new List<NewsArticle>();
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"Fetched {result?.Articles?.Count ?? 0} articles successfully.");

                return result?.Articles ?? new List<NewsArticle>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while calling the News API.");
                return new List<NewsArticle>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing the News API response JSON.");
                return new List<NewsArticle>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching top headlines.");
                return new List<NewsArticle>();
            }
        }
    }
}
