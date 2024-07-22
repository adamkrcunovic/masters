namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusFlightPriceDTO
    {
        public string Currency { get; set; } = string.Empty;
        public string Total { get; set; } = string.Empty;
        public string Base { get; set; } = string.Empty;
        public string GrandTotal { get; set; } = string.Empty;
    }
}