using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using ass1Karan.Models;  

namespace ass1Karan.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public NewsService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<NewsArticle>> GetTopHeadlinesAsync()
        {
            var apiUrl = _config["NewsApi:BaseUrl"];
            var apiKey = _config["NewsApi:ApiKey"];
            var country = _config["NewsApi:Country"];

            string requestUrl = $"{apiUrl}?country={country}&apiKey={apiKey}";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                return new List<NewsArticle>();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NewsApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Articles ?? new List<NewsArticle>();
        }
    }
}
