using Microsoft.Extensions.Caching.Memory;

namespace Global_Logistics_Managemant_System_POE.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;
        }
        
        public async Task<double> GetUSDtoZARRateAsync()
        {
            
            if (_cache.TryGetValue("USDtoZARRate", out double cachedRate))
            {
                return cachedRate;
            }

            var apiKey = _configuration["ApiCurrency:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key is missing.");
            }

            try
            {
                
                var url = $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/USD";

                var response = await _httpClient.GetAsync(url);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"API failed: {response.StatusCode} - {content}");
                }

                var result = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>();

                if (result == null || result.conversion_rates == null || !result.conversion_rates.ContainsKey("ZAR"))
                {
                    throw new InvalidOperationException("Invalid API response.");
                }

                var rate = result.conversion_rates["ZAR"];

                
                _cache.Set("USDtoZARRate", rate, TimeSpan.FromHours(1));

                return rate;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching exchange rate.", ex);
            }
        }

        public async Task<double> ConvertUSDToZARAsync(double amountUSD)
        {
            var rate = await GetUSDtoZARRateAsync();
            return amountUSD * rate;
        }
    }

   
    public class ExchangeRateApiResponse
    {
        public string base_code { get; set; } = string.Empty;
        public Dictionary<string, double> conversion_rates { get; set; } = new();
    }
}