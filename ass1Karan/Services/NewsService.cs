using System;
using System.Net.Http;
using System.Text;
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

        public async Task<(List<NewsArticle> Articles, int TotalResults)> GetTopHeadlinesAsync(
     string category = null, string query = null, int page = 1, int pageSize = 8)
        {
            try
            {
                var apiUrl = _config["NewsApi:BaseUrl"];
                var apiKey = _config["NewsApi:ApiKey"];
                var country = _config["NewsApi:Country"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("News API configuration missing.");
                    return (new List<NewsArticle> { new NewsArticle { Title = "News service configuration error", Description = "Unable to load live news." } }, 0);
                }

                var sb = new System.Text.StringBuilder();
                sb.Append($"{apiUrl}?apiKey={apiKey}");

                if (!string.IsNullOrEmpty(country) && string.IsNullOrEmpty(query))
                    sb.Append($"&country={Uri.EscapeDataString(country)}");

                if (!string.IsNullOrEmpty(category))
                    sb.Append($"&category={Uri.EscapeDataString(category)}");

                if (!string.IsNullOrEmpty(query))
                    sb.Append($"&q={Uri.EscapeDataString(query)}");

                sb.Append($"&page={page}&pageSize={pageSize}");
                var requestUrl = sb.ToString();

                _logger.LogInformation("NewsService requesting: {Url}", requestUrl);

                if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "NewsPortalApp/1.0");
                if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.GetAsync(requestUrl);

                var respBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    string reason = response.ReasonPhrase ?? "Unknown Error";
                    _logger.LogWarning($"News API failed: {response.StatusCode} - {reason}");

                    string userMessage = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "API Key is invalid or expired. Please check credentials.",
                        System.Net.HttpStatusCode.BadRequest => "Invalid request sent to News service. Try changing category or keyword.",
                        System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded. Please wait and try again.",
                        _ => "Unable to fetch live headlines right now. Please try again later."
                    };

                    return (
                        new List<NewsArticle>
                        {
            new NewsArticle
            {
                Title = "Service Unavailable",
                Description = userMessage
            }
                        },
                        0
                    );
                }


                var result = JsonSerializer.Deserialize<NewsApiResponse>(respBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Fetched {Count} articles, totalResults={Total}", result?.Articles?.Count ?? 0, result?.TotalResults ?? 0);

                if (result?.Articles == null || result.Articles.Count == 0)
                    return (new List<NewsArticle> { new NewsArticle { Title = "No News Found", Description = "No top headlines available." } }, 0);

                return (result.Articles, result.TotalResults);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling News API.");
                return (new List<NewsArticle> { new NewsArticle { Title = "Network Error", Description = "Please check your internet connection." } }, 0);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parse error from News API.");
                return (new List<NewsArticle> { new NewsArticle { Title = "Data Parsing Error", Description = "Invalid news data received." } }, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching top headlines.");
                return (new List<NewsArticle> { new NewsArticle { Title = "Unexpected Error", Description = "Something went wrong while fetching news." } }, 0);
            }
        }

    }
}
