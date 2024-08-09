using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using FlightSearch.Constants;
using FlightSearch.Database;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.Extensions.Caching.Memory;

namespace FlightSearch.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ApplicationDbContext _context;

        public FlightRepository(HttpClient httpClient, IMemoryCache memoryCache, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _context = context;
        }

        public async Task<List<OutFlightDealDTO?>> GetFlightData(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs, Boolean multiCity, Boolean flyTheNightBefore)
        {
            var taskList = new List<Task<OutAmadeusFlightSearchDTO?>>();
            var lastInAmadeusFlightSearchDTO = inAmadeusFlightSearchDTOs.Last();
            var dateForLastCall = DateTime.UtcNow;
            dateForLastCall.AddMilliseconds((taskList.Count - 1) * 500 + 10000);
            var token = _memoryCache.Get<OutAmadeusTokenDTO>(ApplicationConstants.TokenCacheString);
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
            var responsesArray = await Task.WhenAll(taskList);
            var responsesList = responsesArray.ToList();
            responsesList = FlightMapper.ShortenResponsesIntoOne(responsesList, multiCity);
            OutAmadeusFlightSearchDTO finalData = new();
            if (responsesList != null)
            {
                for(var i = 0; i < responsesList.Count(); i++) {
                    var response = responsesList[i];
                    if (response != null)
                    {
                        if (flyTheNightBefore && i % 2 == 0) // THE NIGHT BEFORE!!!
                        {
                            response.Data = response.Data.Where(outAmadeusFlightData => outAmadeusFlightData.Itineraries[0].Segments[0].Departure.At.Hour >= 18).ToList();
                        }
                        finalData.Data = finalData.Data.Concat(response.Data).ToList();
                    }
                }
            }
            return finalData.Data.FlightResponseToFlightDeals();
        }

        private async Task<OutAmadeusFlightSearchDTO?> CallAmadeusFlightApi(InAmadeusFlightSearchDTO inAmadeusFlightSearchDTO, string? token)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["originLocationCode"] = inAmadeusFlightSearchDTO.OriginLocationCode;
            query["destinationLocationCode"] = inAmadeusFlightSearchDTO.DestinationLocationCode;
            query["departureDate"] = inAmadeusFlightSearchDTO.DepartureDate.ToString(ApplicationConstants.DateOnlyPattern);
            if (inAmadeusFlightSearchDTO.ReturnDate != null)
            {
                query["returnDate"] = inAmadeusFlightSearchDTO.ReturnDate?.ToString(ApplicationConstants.DateOnlyPattern);
            }
            query["adults"] = inAmadeusFlightSearchDTO.Adults.ToString();
            query["currencyCode"] = inAmadeusFlightSearchDTO.CurrencyCode;
            query["max"] = inAmadeusFlightSearchDTO.Max.ToString();
            var queryString = query.ToString();

            using var request = new HttpRequestMessage(HttpMethod.Get, ApplicationConstants.FlightOffersAmadeusApiAddress + queryString);
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
            using var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.TokenAmadeusApiAddress);
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
                _memoryCache.Set(ApplicationConstants.TokenCacheString, token, new DateTimeOffset(token.ExpiringDate));
            }
            return token;
        }
    }
}