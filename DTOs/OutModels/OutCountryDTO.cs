namespace FlightSearch.DTOs.OutModels
{
    public class OutCountryDTO
    {
        public int Id { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public List<DateOnly> PublicHolidays { get; set; } = new List<DateOnly>();
    }
}