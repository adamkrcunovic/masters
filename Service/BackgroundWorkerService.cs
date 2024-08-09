
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using FlightSearch.Constants;
using FlightSearch.Database;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Helpers;
using FlightSearch.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace FlightSearch.Service
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        public BackgroundWorkerService(IServiceProvider serviceProvider, HttpClient httpClient, IMemoryCache memoryCache)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                await using ApplicationDbContext _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var today = DateHelper.Today;
                var itinerariesToCheck = await _context.Itinenaries
                    .Include(itinerary => itinerary.User)
                    .Include(itinerary => itinerary.Segments)
                    .Include(itinerary => itinerary.InvitedMembers)
                    .ThenInclude(invitedMember => invitedMember.User)
                    .Where(itinerary => itinerary.PriceChangeNotificationType != Enums.PriceChangeNotificationType.NotSet).ToListAsync();
                itinerariesToCheck = itinerariesToCheck.Where(itineraryToCheck => itineraryToCheck.Segments[0].Departure > DateTime.Now).ToList();

                var listOfFlightSearches = new List<InFlightSearchDTO>();
                var listOfSearchesPerItinerary = new List<int>();
                foreach(var savedItinerary in itinerariesToCheck)
                {
                    var toSegments = savedItinerary.Segments.Take(savedItinerary.ToSegmentsLength);
                    var fromSegments = savedItinerary.Segments.TakeLast(savedItinerary.Segments.Count - savedItinerary.ToSegmentsLength);
                    var listOfIncludedAirlines = new List<string>();
                    foreach(var segment in savedItinerary.Segments)
                    {
                        var airlineCode = segment.FlightCode.Split(" - ")[0];
                        if (!listOfIncludedAirlines.Contains(airlineCode))
                        {
                            listOfIncludedAirlines.Add(airlineCode);
                        }
                    }
                    var includedAirlineCodes = "";
                    foreach(var airlineCode in listOfIncludedAirlines)
                    {
                        includedAirlineCodes += airlineCode + ",";
                    }
                    includedAirlineCodes = includedAirlineCodes.Remove(includedAirlineCodes.Length - 1);
                    if (fromSegments.IsNullOrEmpty())
                    {
                        listOfSearchesPerItinerary.Add(1);
                        listOfFlightSearches.Add(new InFlightSearchDTO{
                            FlightSearchType = Enums.FlightSearchType.ExactDate,
                            DepartureDay = DateOnly.FromDateTime(toSegments.First().Departure),
                            Adults = savedItinerary.Adults,
                            FromAirport = toSegments.First().From,
                            ToAirport = toSegments.Last().To,
                            IncludedAirlineCodes = includedAirlineCodes
                        });
                    }
                    else
                    {
                        var isMulticity = toSegments.Last().To != fromSegments.First().From;
                        if (isMulticity)
                        {
                            listOfSearchesPerItinerary.Add(2);
                            listOfFlightSearches.Add(new InFlightSearchDTO{
                                FlightSearchType = Enums.FlightSearchType.ExactDate,
                                DepartureDay = DateOnly.FromDateTime(toSegments.First().Departure),
                                ReturnDay = DateOnly.FromDateTime(fromSegments.First().Departure),
                                Adults = savedItinerary.Adults,
                                FromAirport = toSegments.First().From,
                                MultiCity1 = toSegments.Last().To,
                                MultiCity2 = fromSegments.First().From,
                                ToAirport = fromSegments.Last().To,
                                IncludedAirlineCodes = includedAirlineCodes
                            });
                        }
                        else
                        {
                            listOfSearchesPerItinerary.Add(1);
                            listOfFlightSearches.Add(new InFlightSearchDTO{
                                FlightSearchType = Enums.FlightSearchType.ExactDate,
                                DepartureDay = DateOnly.FromDateTime(toSegments.First().Departure),
                                ReturnDay = DateOnly.FromDateTime(fromSegments.First().Departure),
                                Adults = savedItinerary.Adults,
                                FromAirport = toSegments.First().From,
                                ToAirport = toSegments.Last().To,
                                IncludedAirlineCodes = includedAirlineCodes
                            });
                        }
                    }
                }

                var listOfAmadeusFlightSearches = new List<InAmadeusFlightSearchDTO>();
                foreach(var flightSearch in listOfFlightSearches)
                {
                    var listsToAdd = flightSearch.ToAmadeusFLightSearchDTOs();
                    foreach(var itemToAdd in listsToAdd)
                    {
                        itemToAdd.IncludedAirlineCodes = flightSearch.IncludedAirlineCodes;
                    }
                    listOfAmadeusFlightSearches = listOfAmadeusFlightSearches.Concat(listsToAdd).ToList();
                }
                //Console.WriteLine(JsonSerializer.Serialize(listOfAmadeusFlightSearches));
                var amadeusCallsResponses = await GetFlightData(listOfAmadeusFlightSearches);
                //Console.WriteLine(JsonSerializer.Serialize(amadeusCallsResponses));

                if (amadeusCallsResponses != null)
                {
                    for(int i = 0; i < listOfSearchesPerItinerary.Count(); i++)
                    {
                        var callsNeeded = listOfSearchesPerItinerary[i];
                        var userItinerary = itinerariesToCheck[i];
                        var toSegmentsItinerary = userItinerary.Segments.Take(userItinerary.ToSegmentsLength).ToList();
                        var fromSegmentsItinerary = userItinerary.Segments.TakeLast(userItinerary.Segments.Count - userItinerary.ToSegmentsLength).ToList();
                        
                        var currentPrice = 0d;
                        
                        if (callsNeeded == 1)
                        {
                            var callData = amadeusCallsResponses[0];
                            amadeusCallsResponses.RemoveAt(0);
                            if (callData != null)
                            {
                                var possibilities = callData.Data;
                                if (fromSegmentsItinerary.IsNullOrEmpty())
                                {
                                    //direct flight
                                    foreach(var possibility in possibilities)
                                    {
                                        if (FlightSegmentsHelper.AreSegmentsEqual(toSegmentsItinerary, possibility.Itineraries[0].Segments))
                                        {
                                            Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            currentPrice = Double.Parse(possibility.Price.GrandTotal);
                                        }
                                    }
                                }
                                else
                                {
                                    //return flight
                                    foreach(var possibility in possibilities)
                                    {
                                        if (FlightSegmentsHelper.AreSegmentsEqual(toSegmentsItinerary, possibility.Itineraries[0].Segments) && FlightSegmentsHelper.AreSegmentsEqual(fromSegmentsItinerary, possibility.Itineraries[1].Segments))
                                        {
                                            Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[1].Segments));
                                            currentPrice = Double.Parse(possibility.Price.GrandTotal);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //multicity
                            var callData1 = amadeusCallsResponses[0];
                            amadeusCallsResponses.RemoveAt(0);
                            var callData2 = amadeusCallsResponses[0];
                            amadeusCallsResponses.RemoveAt(0);
                            if (callData1 != null && callData2 != null)
                            {
                                var possibilities1 = callData1.Data;
                                var possibilities2 = callData2.Data;
                                var price1 = 0d;
                                var price2 = 0d;
                                foreach(var possibility in possibilities1)
                                {
                                    if (FlightSegmentsHelper.AreSegmentsEqual(toSegmentsItinerary, possibility.Itineraries[0].Segments))
                                    {
                                        Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                        price1 = Double.Parse(possibility.Price.GrandTotal);
                                    }
                                }
                                if (price1 > 0)
                                {
                                    foreach(var possibility in possibilities2)
                                    {
                                        if (FlightSegmentsHelper.AreSegmentsEqual(fromSegmentsItinerary, possibility.Itineraries[0].Segments))
                                        {
                                            Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            price2 = Double.Parse(possibility.Price.GrandTotal);
                                        }
                                    }
                                    if (price2 > 0)
                                    {
                                        currentPrice = price1 + price2;
                                    }
                                }
                            }
                        }
                        var lastPrice = userItinerary.TotalPrice;
                        var priceDiferenceAmount = lastPrice - currentPrice;
                        var priceDiferencePercent = priceDiferenceAmount / lastPrice;
                        Console.WriteLine($"Old price {lastPrice}, new price {currentPrice}");
                        if (userItinerary.PriceChangeNotificationType == Enums.PriceChangeNotificationType.Amount)
                        {
                            if (Math.Abs(priceDiferenceAmount) > userItinerary.Amount)
                            {
                                if (priceDiferenceAmount > 0)
                                {
                                    // PRICE DROP BY AMOUNT

                                }
                                else
                                {
                                    // PRICE GROW BY AMOUNT
                                }
                            }
                        }
                        if (userItinerary.PriceChangeNotificationType == Enums.PriceChangeNotificationType.Percentage)
                        {
                            if (priceDiferencePercent > (userItinerary.Percentage / 100))
                            {
                                if (priceDiferenceAmount > 0)
                                {
                                    // PRICE DROP BY PERCENT
                                }
                                else
                                {
                                    // PRICE GROW BY PERCENT
                                }
                            }
                        }
                    }
                }
                await Task.Delay(24*60*60*1000, stoppingToken);
            }
        }

        public async Task<List<OutAmadeusFlightSearchDTO?>?> GetFlightData(List<InAmadeusFlightSearchDTO> inAmadeusFlightSearchDTOs)
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
            return responsesList;
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