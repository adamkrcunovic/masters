
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using FlightSearch.Constants;
using FlightSearch.Database;
using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Helpers;
using FlightSearch.Mappers;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
                    .ToListAsync();
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
                                            //Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            currentPrice = Double.Parse(possibility.Price.GrandTotal);

                                            userItinerary.CurrentPrice = currentPrice;
                                            await _context.SaveChangesAsync();
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
                                            //Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            //(JsonSerializer.Serialize(possibility.Itineraries[1].Segments));
                                            currentPrice = Double.Parse(possibility.Price.GrandTotal);

                                            userItinerary.CurrentPrice = currentPrice;
                                            await _context.SaveChangesAsync();
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
                                        //Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                        price1 = Double.Parse(possibility.Price.GrandTotal);
                                    }
                                }
                                if (price1 > 0)
                                {
                                    foreach(var possibility in possibilities2)
                                    {
                                        if (FlightSegmentsHelper.AreSegmentsEqual(fromSegmentsItinerary, possibility.Itineraries[0].Segments))
                                        {
                                            //Console.WriteLine(JsonSerializer.Serialize(possibility.Itineraries[0].Segments));
                                            price2 = Double.Parse(possibility.Price.GrandTotal);
                                        }
                                    }
                                    if (price2 > 0)
                                    {
                                        currentPrice = price1 + price2;

                                        userItinerary.CurrentPrice = currentPrice;
                                        await _context.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                        var lastPrice = userItinerary.TotalPrice;
                        var priceDiferenceAmount = lastPrice - currentPrice;
                        var priceDiferencePercent = priceDiferenceAmount / lastPrice;
                        if (userItinerary.PriceChangeNotificationType == Enums.PriceChangeNotificationType.Amount)
                        {
                            if (Math.Abs(priceDiferenceAmount) > userItinerary.Amount)
                            {
                                await SendNotificationForItinerary(userItinerary, true, priceDiferenceAmount);
                            }
                        }
                        if (userItinerary.PriceChangeNotificationType == Enums.PriceChangeNotificationType.Percentage)
                        {
                            if (priceDiferencePercent > (userItinerary.Percentage / 100))
                            {
                                await SendNotificationForItinerary(userItinerary, false, priceDiferencePercent);
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

        private String? GetBearerToken() {
            String firebaseMessagingScope = "https://www.googleapis.com/auth/firebase.messaging";
            String jsonString = "{\n" +
                "  \"type\": \"service_account\",\n" +
                "  \"project_id\": \"masters-362af\",\n" +
                "  \"private_key_id\": \"2b4da373b20549c3ef783aa9df45093de77ce167\",\n" +
                "  \"private_key\": \"-----BEGIN PRIVATE KEY-----\\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDamGL6A4Jmojau\\n7z25cDWlq6vxJFoE1xeAoExoW1M60vAJr5vSeEsgXa/UWu/ALDTWP1BJbL9uIMJQ\\n+atcwVSOl5qVB9TIpQKYmRndaJfGTbDj+il4X5j4dSJUbPfx63C3h7ttXurOlW38\\n8OLDvWApo8UsSFFAHE7V2LX4L+ZSXLTKzK4k40Uujt9yiZjP2erL5pDMKOt/gllI\\n8Wy0r6TN+AqbCrstq7ktB3F+ZlqtTnY80pzYt/97+kb3uwQU3cAD479H7CwYKCzd\\nJgNYmqBmkzIeDGQfOzfAUZHCfYbfAnBXe4TglkmrDYuKKdKay4cRuI/nVx2cj0la\\nlenGKWTRAgMBAAECggEAIZcWaSzDi68+5UAberS6HwHI+NPehq02uKvpPIVWviH5\\nLhkvbKF83zWw9doeH/2rBjEnI+FIw/eCD0djrxaA6S3KselI0qbomze2OHEAwZbh\\nU7xL8GoK65he9MePN5GM+ebVmkeM9Wm+Q7FlvIZhPITPS7AL1uTYCGxgqglSRIge\\n5OHw+tTJreHsKF9EIn8hA4zJdWJ23IWzBT7AHOgP6TrvI5m9l3Tde5ZltPkmMd+M\\nBNJy3z8KHvJuxxTSfkV/mUmauzoiMuOuqoxoZif7JX/nqXE4zJpDzTjlfAx8wFt4\\nffgnK5zdB4Upj0sHpUTlU9uisZQzTOP82IFa0XMqMQKBgQD7SW4bN54G+gzjcdMC\\nBbai1Taqe3Cr9rW8LRFJ+XvOdRENcyiOb/L49fmHqYSOYhd5EJwLJQTL3VVqdN/h\\nrlQUxC59m/zUfCgdQnxjzrFoHo5rhczs3wmJcMOEexNIBfEQd/VEpBO/0k/j89LG\\nPOiP6ZNgxyqVUX0h6CsQeRPq/wKBgQDesfxevp8dzXJgERk9tKr4cstcprXV4RQS\\n0KduQWI3lvUUTwIskzljTPPq91A2Gew9J6R+nExvSkSpKE1IWcs9ZV4MGScYXxRB\\ninN0KEDWDaTK93eSJmQJbi1aDu6DQhC1fHhkrZzbjg0KV58ZgovCKV/1ea6G14uE\\ngb/Nzr7ALwKBgEtweRDMurGHgjUKJ/n0cychcX7u/h1yPI8YzJbzwjpyJMNv7h4M\\n99nMJrSWrMf+JOPgm6gw3ebCNPF30vqy1mVBnF9zZAz6lSRroGJqXBJREhqvmZ0H\\nPJq5cskkFd7Kgdua19Ramd89qWRa/80p3fvOeMNWJ6+aPkHerIcOgm9LAoGBAKl/\\n21DZ0g5DA10vZoDa9I7qAPNiSGCkUj0H54g55+Hb2mo8wLDg1ftI5RbgaoLjNDZP\\n6BoeKOdEJgKClGAPSGxQrUaUFnesVqSUFtBAmyjRda6usKni4p1y6L31Q4FQVZtt\\nQ82Nfyh1dGN80bH+9RUxnMIgfcBQavbOMwkY5YMtAoGAMJZGGmTG+MG9uSJ7dqu3\\nlhA7nJ4BSVpGQqR1Aegc7cPs5FikQHKGXN96OcXtaHoORTGsQ4SNwT8IHMNTyYw1\\n9Ri25VFLI1mIt22KeJRdD0YnePyKvrXD9J8ftJ0NSdAPsoa+XASu3VCHvGk44I4O\\nHuSo30FEU58hYJSFvhRWNI4=\\n-----END PRIVATE KEY-----\\n\",\n" +
                "  \"client_email\": \"firebase-adminsdk-f0ton@masters-362af.iam.gserviceaccount.com\",\n" +
                "  \"client_id\": \"104237415465056325227\",\n" +
                "  \"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\n" +
                "  \"token_uri\": \"https://oauth2.googleapis.com/token\",\n" +
                "  \"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\n" +
                "  \"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-f0ton%40masters-362af.iam.gserviceaccount.com\",\n" +
                "  \"universe_domain\": \"googleapis.com\"\n" +
                "}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var scopes = new List<string> { firebaseMessagingScope };
            var googleCredential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            var accessToken = googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;
            return accessToken;
        }

        public async Task SendNotification(string title, string body, string deviceId) {
            var bearerToken = GetBearerToken();
            using var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.GoogleCloudMessagingApiAddress);
            var bodyJson = "{\n"+
                " \"message\": {\n" +
                " \"token\": \"" + deviceId + "\",\n" +
                " \"notification\": {\n" +
                " \"title\": \"" + title + "\",\n" +
                " \"body\": \"" + body + "\"\n" + 
                "}\n" +
                "}\n" +
                "}\n";
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            await _httpClient.SendAsync(request);
        }

        public async Task SendNotificationForItinerary(Itinerary itinerary, Boolean amountOrPercentage, double value) {
            var devices = itinerary.User.DeviceIds.Split(";").Where(deviceId => !deviceId.IsNullOrEmpty()).ToList();
            foreach(var invitedMemer in itinerary.InvitedMembers) {
                devices = devices.Concat(invitedMemer.User.DeviceIds.Split(";").Where(deviceId => !deviceId.IsNullOrEmpty()).ToList()).ToList();
            }
            foreach(var deviceId in devices) {
                await SendNotification("Flight tickets price alert", "The price has gone " + (value > 0 ? "up" : "down") + " for " + Math.Abs(value).ToString() + (amountOrPercentage ? "euros" : "%"), deviceId);
            }
        }
    }
}