namespace FlightSearch.DTOs.ThirdPartyModels.OutModels
{
    public class OutAmadeusAirportDTO
    {
        public string Name { get; set; } = string.Empty;
        public string IataCode { get; set; } = string.Empty;
        public OutAmadeusAirportAddressDTO Address { get; set; } = new OutAmadeusAirportAddressDTO();

        public void SetNameWithCountry() {
            Name = Name + ", " + Address.CountryName;
        }
    }
}