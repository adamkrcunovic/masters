using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.Enums;

namespace FlightSearch.Mappers
{
    public static class FlightMapper
    {
        public static List<InAmadeusFlightSearchDTO> ToAmadeusFLightSearchDTOs(this InFlightSearchDTO inFlightSearchDTO)
        {
            var requestsList = new List<InAmadeusFlightSearchDTO>();
            if (!inFlightSearchDTO.OneWay)
            {
                switch(inFlightSearchDTO.FlightSearchType)
                {
                    case FlightSearchType.ExactDate:
                    {
                        requestsList.Add(FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, inFlightSearchDTO.DepartureDay.GetValueOrDefault(), inFlightSearchDTO.ReturnDay));
                        break;
                    }
                    case FlightSearchType.DuratinInMonth:
                    case FlightSearchType.LongWeekendInMonth:
                    case FlightSearchType.DoubleLongWeekendInMonth:
                    {
                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var dayForLoop = new DateOnly(inFlightSearchDTO.Year.GetValueOrDefault(), inFlightSearchDTO.Month.GetValueOrDefault(), inFlightSearchDTO.Year == today.Year && inFlightSearchDTO.Month == today.Month ? today.Day : 1);
                        var tripDuration = 0;
                        switch (inFlightSearchDTO.FlightSearchType)
                        {
                            case FlightSearchType.DuratinInMonth:
                            {
                                tripDuration = inFlightSearchDTO.TripDuration.GetValueOrDefault();
                                break;
                            }
                            case FlightSearchType.LongWeekendInMonth:
                            {
                                tripDuration = 4;
                                break;
                            }
                            case FlightSearchType.DoubleLongWeekendInMonth:
                            {
                                tripDuration = 8;
                                break;
                            }
                        }
                        do 
                        {
                            var dayOfWeek = dayForLoop.DayOfWeek;
                            var capableForLongWeekend = dayOfWeek == DayOfWeek.Thursday || dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday;
                            var capableForDoubleWeekend = dayOfWeek == DayOfWeek.Saturday;
                            if (inFlightSearchDTO.FlightSearchType == FlightSearchType.DuratinInMonth || 
                                (inFlightSearchDTO.FlightSearchType == FlightSearchType.LongWeekendInMonth && capableForLongWeekend) ||
                                (inFlightSearchDTO.FlightSearchType == FlightSearchType.DoubleLongWeekendInMonth && capableForDoubleWeekend))
                            {
                                requestsList.Add(FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, dayForLoop, dayForLoop.AddDays(tripDuration - 1)));
                            }
                            dayForLoop = dayForLoop.AddDays(1);
                        }
                        while (dayForLoop.Day != 1);
                        break;
                    }
                }
            }
            if (inFlightSearchDTO.FlyTheNightBefore)
            {
                var shortListCount = requestsList.Count;
                for (var i = 0; i < shortListCount; i++)
                {
                    requestsList.Insert(
                        i*2,
                        FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, requestsList[i*2].DepartureDate.AddDays(-1), requestsList[i*2].ReturnDate)
                    );
                }
                if (requestsList[0].DepartureDate.Day != 1) requestsList.RemoveAt(0);
            }
            requestsList.ForEach(request => request.Max = 250 / requestsList.Count);
            return requestsList;
        }

        public static InAmadeusFlightSearchDTO FromDatePairToAmadeusFlightSearch(InFlightSearchDTO inFlightSearchDTO, DateOnly DepartureDate, DateOnly? ReturnDate)
        {
            return new InAmadeusFlightSearchDTO(){
                OriginLocationCode = inFlightSearchDTO.FromAirport,
                DestinationLocationCode = inFlightSearchDTO.ToAirport,
                DepartureDate = DepartureDate,
                ReturnDate = ReturnDate,
                Adults = inFlightSearchDTO.Adults
            };
        }
    }
}