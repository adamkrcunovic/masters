using FlightSearch.Database.Models;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;

namespace FlightSearch.Helpers
{
    public static class FlightSegmentsHelper
    {
        public static bool AreSegmentsEqual(List<FlightSegment> flightSegments, List<OutAmadeusFlightSegmentDTO> amadeusFlightSegments)
        {
            if (flightSegments.Count() == amadeusFlightSegments.Count())
            {
                for(int i = 0; i < flightSegments.Count(); i++)
                {
                    var flightCode1 = flightSegments[i].FlightCode;
                    var flightCode2 = amadeusFlightSegments[i].CarrierCode + " - " + amadeusFlightSegments[i].Number;
                    if (flightCode1 != flightCode2) return false;
                }
                return true;
            }
            return false;
        }
    }
}