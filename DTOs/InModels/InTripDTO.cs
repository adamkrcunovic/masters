using FlightSearch.DTOs.OutModels;
using FlightSearch.Helpers;

namespace FlightSearch.DTOs.InModels
{
    public class InTripDTO
    {
        public List<OutFlightSegmentDTO> ToSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<String> LayoverToDuration { get; set; } = new List<String>();
        public List<OutFlightSegmentDTO> FromSegments { get; set; } = new List<OutFlightSegmentDTO>();
        public List<String>? LayoverFromDuration { get; set; }
    }
}