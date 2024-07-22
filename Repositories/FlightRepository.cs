using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Web;
using FlightSearch.Database;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FlightSearch.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public FlightRepository(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<Object?> GetFlightData(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs)
        {
            var returnData = await MakeFlightApiCall(inAmadeusFlightSearchDTOs);
            return returnData;
        }

        private async Task<Object?> MakeFlightApiCall(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs)
        {
            var taskList = new List<Task<OutAmadeusFlightSearchDTO?>>();
            var lastInAmadeusFlightSearchDTO = inAmadeusFlightSearchDTOs.Last();
            var dateForLastCall = DateTime.UtcNow;
            dateForLastCall.AddMilliseconds((taskList.Count - 1) * 500 + 10000);
            var token = _memoryCache.Get<OutAmadeusTokenDTO>("token");
            if (token == null || dateForLastCall > token.ExpiringDate)
            {
                token = await AmadeusAuthorization();
                await Task.Delay(500);
            }
            foreach (var inAmadeusFlightSearchDTO in inAmadeusFlightSearchDTOs)
            {
                taskList.Add(CallAmadeusFlightApi(inAmadeusFlightSearchDTO, token?.AccessToken));
                if (inAmadeusFlightSearchDTO != lastInAmadeusFlightSearchDTO) await Task.Delay(500);
            }
            var responses = await Task.WhenAll(taskList);
            return responses;
        }

        private async Task<OutAmadeusFlightSearchDTO?> CallAmadeusFlightApi(InAmadeusFlightSearchDTO inAmadeusFlightSearchDTO, string? token)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["originLocationCode"] = inAmadeusFlightSearchDTO.OriginLocationCode;
            query["destinationLocationCode"] = inAmadeusFlightSearchDTO.DestinationLocationCode;
            query["departureDate"] = inAmadeusFlightSearchDTO.DepartureDate.ToString("yyyy-MM-dd");
            if (inAmadeusFlightSearchDTO.ReturnDate != null)
            {
                query["returnDate"] = inAmadeusFlightSearchDTO.ReturnDate?.ToString("yyyy-MM-dd");
            }
            query["adults"] = inAmadeusFlightSearchDTO.Adults.ToString();
            query["currencyCode"] = inAmadeusFlightSearchDTO.CurrencyCode;
            query["max"] = inAmadeusFlightSearchDTO.Max.ToString();
            var queryString = query.ToString();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.api.amadeus.com/v2/shopping/flight-offers?" + queryString);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var returnModel = JsonSerializer.Deserialize<OutAmadeusFlightSearchDTO>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return returnModel;
        }

        private async Task<OutAmadeusTokenDTO?> AmadeusAuthorization()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v1/security/oauth2/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>{
                {"grant_type", "client_credentials"},
                {"client_id", "Gfsuj2z8PmPA9cDxC9NQAYiib0PApGDB"},
                {"client_secret", "wYGuCeWAoPLVavYO"}
            });
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<OutAmadeusTokenDTO>(responseString, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
            token?.SetExpirationDate();
            if (token != null)
            {
                _memoryCache.Set("token", token, new DateTimeOffset(token.ExpiringDate));
            }
            return token;
        }
    }
}