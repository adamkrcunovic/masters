namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutOpenAIChatDTO
    {
        public List<OutOpenAIChoiceDTO> Choices { get; set; } = new List<OutOpenAIChoiceDTO>();
    }
}