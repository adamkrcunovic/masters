using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlightSearch.Constants;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Interfaces;

namespace FlightSearch.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly HttpClient _httpClient;

        public TripRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetChatGPTData(string inputText)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.OpenAIApiAddress);
            var body = "{\"model\":\"gpt-4o\",\"messages\":[{\"role\":\"user\",\"content\":\"" + inputText + "\"}]}";
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApplicationConstants.OpenAIApiToken);
            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var returnData = JsonSerializer.Deserialize<OutOpenAIChatDTO>(responseString, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            return returnData != null ? returnData.Choices[0].Message.Content : "";
        }
    }
}