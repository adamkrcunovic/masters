using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Enums;
using FlightSearch.Helpers;

namespace FlightSearch.Mappers
{
    public static class FlightMapper
    {     
        public static List<InAmadeusFlightSearchDTO> ToAmadeusFLightSearchDTOs(this InFlightSearchDTO inFlightSearchDTO)
        {
            var requestsList = new List<InAmadeusFlightSearchDTO>();
            var today = DateHelper.Today();
            switch(inFlightSearchDTO.FlightSearchType)
                {
                    case FlightSearchType.ExactDate:
                    {
                        if (inFlightSearchDTO.MultiCity1 == null || inFlightSearchDTO.MultiCity1.Length == 0)
                        {
                            requestsList.Add(FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, inFlightSearchDTO.DepartureDay.GetValueOrDefault(), inFlightSearchDTO.ReturnDay));
                        }
                        else
                        {
                            requestsList.Add(FromDatePairMulticityToAmadeusFlightSearch(inFlightSearchDTO, true, inFlightSearchDTO.DepartureDay.GetValueOrDefault()));
                            requestsList.Add(FromDatePairMulticityToAmadeusFlightSearch(inFlightSearchDTO, false, inFlightSearchDTO.ReturnDay.GetValueOrDefault()));
                            if (inFlightSearchDTO.MixMultiCity == true) {
                                requestsList.Add(FromDatePairMulticityMixToAmadeusFlightSearch(inFlightSearchDTO, true, inFlightSearchDTO.DepartureDay.GetValueOrDefault()));
                                requestsList.Add(FromDatePairMulticityMixToAmadeusFlightSearch(inFlightSearchDTO, false, inFlightSearchDTO.ReturnDay.GetValueOrDefault()));
                            }
                        }
                        break;
                    }
                    case FlightSearchType.MonthDirectFlight:
                    {
                        var dayForLoop = new DateOnly(inFlightSearchDTO.Year.GetValueOrDefault(), inFlightSearchDTO.Month.GetValueOrDefault(), inFlightSearchDTO.Year == today.Year && inFlightSearchDTO.Month == today.Month ? today.Day : 1);            
                        do 
                        {
                            var dayOfWeek = dayForLoop.DayOfWeek;
                            requestsList.Add(FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, dayForLoop, null));
                            dayForLoop = dayForLoop.AddDays(1);
                        }
                        while (dayForLoop.Day != 1);
                        break;
                    }
                    case FlightSearchType.DuratinInMonth:
                    case FlightSearchType.LongWeekendInMonth:
                    case FlightSearchType.DoubleLongWeekendInMonth:
                    {
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
                        var dayForLoop = new DateOnly(inFlightSearchDTO.Year.GetValueOrDefault(), inFlightSearchDTO.Month.GetValueOrDefault(), inFlightSearchDTO.Year == today.Year && inFlightSearchDTO.Month == today.Month ? today.Day : 1);            
                        do 
                        {
                            var dayOfWeek = dayForLoop.DayOfWeek;
                            var capableForLongWeekend = dayOfWeek == DayOfWeek.Thursday || dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday;
                            var capableForDoubleWeekend = dayOfWeek == DayOfWeek.Saturday;
                            if (inFlightSearchDTO.FlightSearchType == FlightSearchType.DuratinInMonth || 
                                (inFlightSearchDTO.FlightSearchType == FlightSearchType.LongWeekendInMonth && capableForLongWeekend) ||
                                (inFlightSearchDTO.FlightSearchType == FlightSearchType.DoubleLongWeekendInMonth && capableForDoubleWeekend))
                            {
                                if (inFlightSearchDTO.MultiCity1 == null || inFlightSearchDTO.MultiCity1.Length == 0)
                                {
                                    requestsList.Add(FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, dayForLoop, dayForLoop.AddDays(tripDuration - 1)));
                                }
                                else
                                {
                                    requestsList.Add(FromDatePairMulticityToAmadeusFlightSearch(inFlightSearchDTO, true, dayForLoop));
                                    requestsList.Add(FromDatePairMulticityToAmadeusFlightSearch(inFlightSearchDTO, false, dayForLoop.AddDays(tripDuration - 1)));
                                    if (inFlightSearchDTO.MixMultiCity == true) {
                                        requestsList.Add(FromDatePairMulticityMixToAmadeusFlightSearch(inFlightSearchDTO, true, dayForLoop));
                                        requestsList.Add(FromDatePairMulticityMixToAmadeusFlightSearch(inFlightSearchDTO, false, dayForLoop.AddDays(tripDuration - 1)));
                                    }
                                }
                            }
                            dayForLoop = dayForLoop.AddDays(1);
                        }
                        while (dayForLoop.Day != 1);
                        break;
                    }
                }
            if (inFlightSearchDTO.FlyTheNightBefore)    //NOT EXISTING FOR MULTICITY
            {
                var shortListCount = requestsList.Count;
                for (var i = 0; i < shortListCount; i++)
                {
                    requestsList.Insert(
                        i*2,
                        FromDatePairToAmadeusFlightSearch(inFlightSearchDTO, requestsList[i*2].DepartureDate.AddDays(-1), requestsList[i*2].ReturnDate)
                    );
                }
            }
            requestsList.ForEach(request => request.Max = 250 / requestsList.Count);
            return requestsList;
        }

        public static InAmadeusFlightSearchDTO FromDatePairToAmadeusFlightSearch(InFlightSearchDTO inFlightSearchDTO, DateOnly DepartureDate, DateOnly? ReturnDate)
        {
            return new InAmadeusFlightSearchDTO() {
                OriginLocationCode = inFlightSearchDTO.FromAirport,
                DestinationLocationCode = inFlightSearchDTO.ToAirport,
                DepartureDate = DepartureDate,
                ReturnDate = ReturnDate,
                Adults = inFlightSearchDTO.Adults
            };
        }

        public static InAmadeusFlightSearchDTO FromDatePairMulticityToAmadeusFlightSearch(InFlightSearchDTO inFlightSearchDTO, bool toDirection, DateOnly DepartureDate)
        {
            return new InAmadeusFlightSearchDTO() {
                OriginLocationCode = toDirection ? inFlightSearchDTO.FromAirport : inFlightSearchDTO.MultiCity2 == null ? "" : inFlightSearchDTO.MultiCity2,
                DestinationLocationCode = toDirection ? inFlightSearchDTO.MultiCity1 == null ? "" : inFlightSearchDTO.MultiCity1 : inFlightSearchDTO.ToAirport,
                DepartureDate = DepartureDate,
                ReturnDate = null,
                Adults = inFlightSearchDTO.Adults
            };
        }

        public static InAmadeusFlightSearchDTO FromDatePairMulticityMixToAmadeusFlightSearch(InFlightSearchDTO inFlightSearchDTO, bool toDirection, DateOnly DepartureDate)
        {
            return new InAmadeusFlightSearchDTO() {
                OriginLocationCode = toDirection ? inFlightSearchDTO.FromAirport : inFlightSearchDTO.MultiCity1 == null ? "" : inFlightSearchDTO.MultiCity1,
                DestinationLocationCode = toDirection ? inFlightSearchDTO.MultiCity2 == null ? "" : inFlightSearchDTO.MultiCity2 : inFlightSearchDTO.ToAirport,
                DepartureDate = DepartureDate,
                ReturnDate = null,
                Adults = inFlightSearchDTO.Adults
            };
        }

        public static List<OutFlightDealDTO?> FlightResponseToFlightDeals (this List<OutAmadeusFlightDataDTO?>? outAmadeusFlightDataDTOs)
        {
            var returnList = new List<OutFlightDealDTO?>();
            if (outAmadeusFlightDataDTOs != null)
            {
                foreach (var amadeusData in outAmadeusFlightDataDTOs)
                {
                    if (amadeusData != null)
                    {
                    returnList.Add(new OutFlightDealDTO() {
                        ToDuration = DateHelper.RenameHourString(amadeusData.Itineraries[0].Duration),
                        ToSegments = amadeusData.Itineraries[0].Segments.OutAmadeusSegmentToOutFlightSegment(),
                        LayoverToDuration = amadeusData.Itineraries[0].Segments.OutAmadeusSegmentToLayouvers(),
                        FromDuration = amadeusData.Itineraries.Count == 2 ? DateHelper.RenameHourString(amadeusData.Itineraries[1].Duration) : null,
                        FromSegments = amadeusData.Itineraries.Count == 2 ? amadeusData.Itineraries[1].Segments.OutAmadeusSegmentToOutFlightSegment() : null,
                        LayoverFromDuration = amadeusData.Itineraries.Count == 2 ? amadeusData.Itineraries[1].Segments.OutAmadeusSegmentToLayouvers() : null,
                        TotalPrice = Double.Parse(amadeusData.Price.Total)
                    });
                    }
                    else
                    {
                        returnList.Add(null);
                    }
                }
            }
            return returnList;
        }

        public static List<OutFlightSegmentDTO> OutAmadeusSegmentToOutFlightSegment (this List<OutAmadeusFlightSegmentDTO> outAmadeusFlightSegmentDTOs)
        {
            var returnList = new List<OutFlightSegmentDTO>();
            foreach (var amadeusSegment in outAmadeusFlightSegmentDTOs)
            {
                returnList.Add(new OutFlightSegmentDTO{
                    From = amadeusSegment.Departure.IataCode,
                    To = amadeusSegment.Arrival.IataCode,
                    Departure = amadeusSegment.Departure.At,
                    Arrival = amadeusSegment.Arrival.At,
                    Duration = DateHelper.RenameHourString(amadeusSegment.Duration),
                    FlightCode = amadeusSegment.CarrierCode + " - " + amadeusSegment.Number
                });
            }
            return returnList;
        }

        public static List<string> OutAmadeusSegmentToLayouvers (this List<OutAmadeusFlightSegmentDTO> outAmadeusFlightSegmentDTOs)
        {
            var returnList = new List<string>();
            for (var i = 1; i < outAmadeusFlightSegmentDTOs.Count; i++)
            {
                TimeSpan dateDiff = outAmadeusFlightSegmentDTOs[i].Departure.At - outAmadeusFlightSegmentDTOs[i - 1].Arrival.At;
                var hours = dateDiff.Hours;
                var minutes = dateDiff.Minutes % 60;
                returnList.Add((hours > 0 ? (hours > 1 ? hours + "Hours " : "1Hour ") : "") + (minutes > 0 ? minutes + "Minutes" : ""));
            }
            return returnList;
        }

    }
}