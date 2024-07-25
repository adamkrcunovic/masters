using FlightSearch.Helpers;

namespace FlightSearch.DTOs.OutModels
{
    public class OutFlightDealDTO
    {

        //public string TotalFlightsDuration { get; set; } = string.Empty;
        private int TotalFlightsDurationInMinutes { get; set; }
        public string? TotalStayDuration { get; set; }
        private int? TotalStayDurationInMinutes { get; set; }
        public string ToDuration { get; set; } = string.Empty;
        public List<OutFlightSegmentDTO> ToSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<String> LayoverToDuration { get; set; } = new List<String>();
        public List<String> CityVisit { get; set; } = new List<String>();
        public string? FromDuration { get; set; }
        public List<OutFlightSegmentDTO>? FromSegments { get; set; }
        public List<String>? LayoverFromDuration { get; set; }
        public Double TotalPrice { get; set; }
        public int ReturnTotalFlightsDurationInMinutes()
        {
            return TotalFlightsDurationInMinutes;
        }
        public int? ReturnTotalStayDurationInMinutes()
        {
            return TotalStayDurationInMinutes;
        }
        public void SetFlightAndStayDuration()
        {
            TotalFlightsDurationInMinutes = DateHelper.MinutesDifference(ToSegments[ToSegments.Count - 1].Arrival, ToSegments[0].Departure) + (FromSegments != null ? DateHelper.MinutesDifference(FromSegments[FromSegments.Count - 1].Arrival, FromSegments[0].Departure) : 0);
            //TotalFlightsDuration = DateHelper.MinutesToTimeFormat(TotalFlightsDurationInMinutes);
            if (FromSegments != null)
            {
                TotalStayDurationInMinutes = DateHelper.MinutesDifference(FromSegments[0].Departure, ToSegments[ToSegments.Count - 1].Arrival);
                TotalStayDuration = DateHelper.MinutesToTimeFormatWithDays(TotalStayDurationInMinutes ?? default);
            }
        }
    }
}