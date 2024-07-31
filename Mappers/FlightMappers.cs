using FlightSearch.Database.Models;
using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.DTOs.ThirdPartyModels.InModels;
using FlightSearch.DTOs.ThirdPartyModels.OutModels;
using FlightSearch.Enums;
using FlightSearch.Helpers;
using Microsoft.IdentityModel.Tokens;

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
                        if (!inFlightSearchDTO.MulticityDefined())
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

        public static OutFlightDTO ToFinalFlightData(this List<OutFlightDealDTO?> flightDeals)
        {
            flightDeals.RemoveAll(item => item == null);
            return new OutFlightDTO{
                CheapestFlights = flightDeals.OrderBy(flightDeal => flightDeal?.TotalPrice).Take(10).ToList(),
                FastestFlights = flightDeals.OrderBy(flightDeal => flightDeal?.ReturnTotalFlightsDurationInMinutes()).ThenBy(flightDeal => flightDeal?.TotalPrice).Take(10).ToList(),
                LongestStayFlights = flightDeals.Where(flightDeals => flightDeals?.FromDuration != null).OrderByDescending(flightDeal => flightDeal?.ReturnTotalStayDurationInMinutes()).Take(10).ToList(),
                CityVisit = flightDeals.Where(flightDeals => flightDeals?.CityVisit.Count > 0).OrderByDescending(flightDeal => flightDeal?.CityVisit.Count).ThenBy(flightDeal => flightDeal?.TotalPrice).Take(10).ToList(),
            };
        }

        public static List<OutFlightDealDTO?> FlightResponseToFlightDeals (this List<OutAmadeusFlightDataDTO> outAmadeusFlightDataDTOs)
        {
            var returnList = new List<OutFlightDealDTO?>();
            if (outAmadeusFlightDataDTOs != null)
            {
                foreach (var amadeusData in outAmadeusFlightDataDTOs)
                {
                    if (amadeusData != null)
                    {
                        var flightData = new OutFlightDealDTO() {
                            ToDuration = DateHelper.RenameHourString(amadeusData.Itineraries[0].Duration),
                            ToSegments = amadeusData.Itineraries[0].Segments.OutAmadeusSegmentToOutFlightSegment(),
                            LayoverToDuration = amadeusData.Itineraries[0].Segments.OutAmadeusSegmentToLayovers(),
                            CityVisit = amadeusData.Itineraries[0].Segments.OutAmadeusSegmentToLayoversCityVisit().Concat(amadeusData.Itineraries.Count == 2 ? amadeusData.Itineraries[1].Segments.OutAmadeusSegmentToLayoversCityVisit() : new List<string>()).ToList(),
                            FromDuration = amadeusData.Itineraries.Count == 2 ? DateHelper.RenameHourString(amadeusData.Itineraries[1].Duration) : null,
                            FromSegments = amadeusData.Itineraries.Count == 2 ? amadeusData.Itineraries[1].Segments.OutAmadeusSegmentToOutFlightSegment() : null,
                            LayoverFromDuration = amadeusData.Itineraries.Count == 2 ? amadeusData.Itineraries[1].Segments.OutAmadeusSegmentToLayovers() : null,
                            TotalPrice = Double.Parse(amadeusData.Price.Total)
                        };
                        flightData.SetFlightAndStayDuration();
                        returnList.Add(flightData);
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

        public static List<string> OutAmadeusSegmentToLayovers (this List<OutAmadeusFlightSegmentDTO> outAmadeusFlightSegmentDTOs)
        {
            var returnList = new List<string>();
            for (var i = 1; i < outAmadeusFlightSegmentDTOs.Count; i++)
            {
                returnList.Add(DateHelper.LayoverStringInFormat(outAmadeusFlightSegmentDTOs[i].Departure.At, outAmadeusFlightSegmentDTOs[i - 1].Arrival.At));
            }
            return returnList;
        }

        public static List<String> OutAmadeusSegmentToLayoversCityVisit (this List<OutAmadeusFlightSegmentDTO> outAmadeusFlightSegmentDTOs)
        {
            var returnList = new List<String>();
            for (var i = 1; i < outAmadeusFlightSegmentDTOs.Count; i++)
            {
                if (DateHelper.MinutesDifference(outAmadeusFlightSegmentDTOs[i].Departure.At, outAmadeusFlightSegmentDTOs[i - 1].Arrival.At) > 720)
                returnList.Add(outAmadeusFlightSegmentDTOs[i].Departure.IataCode);
            }
            return returnList;
        }

        public static OutAmadeusFlightSearchDTO? MergeTwoMulticityItineraries(OutAmadeusFlightSearchDTO? data1, OutAmadeusFlightSearchDTO? data2)
        {
            if (data1 != null && data2 != null)
            {
                data1.Data = data1.Data.OrderBy(data => data.Price.Total).ToList();
                data2.Data = data2.Data.OrderBy(data => data.Price.Total).ToList();
                var numberOfItineraries = data1.Data.Count;
                var outAmadeusFlightSearchDTO = new OutAmadeusFlightSearchDTO();
                for(int i = 0; i < data1.Data.Count; i++)
                {
                    for(int j = 0; j < data2.Data.Count; j++)
                    {
                        outAmadeusFlightSearchDTO.Data.Add(new OutAmadeusFlightDataDTO{
                            Itineraries = new List<OutAmadeusFlightItineraryDTO>{
                                data1.Data[i].Itineraries[0],
                                data2.Data[j].Itineraries[0]
                            },
                            Price = new OutAmadeusFlightPriceDTO(){
                                Currency = data1.Data[i].Price.Currency,
                                Total = (Convert.ToDouble(data1.Data[i].Price.Total) + Convert.ToDouble(data2.Data[j].Price.Total)).ToString(),
                                Base = (Convert.ToDouble(data1.Data[i].Price.Base) + Convert.ToDouble(data2.Data[j].Price.Base)).ToString(),
                                GrandTotal = (Convert.ToDouble(data1.Data[i].Price.GrandTotal) + Convert.ToDouble(data2.Data[j].Price.GrandTotal)).ToString(),
                            }
                        });
                    }
                }
                outAmadeusFlightSearchDTO.Data = outAmadeusFlightSearchDTO.Data.OrderBy(data => Convert.ToDouble(data.Price.Total)).Take(numberOfItineraries * 2).ToList();
                return outAmadeusFlightSearchDTO;
            }
            else
            {
                return null;
            }
        }

        public static List<OutAmadeusFlightSearchDTO?>? ShortenResponsesIntoOne(List<OutAmadeusFlightSearchDTO?> responsesList, bool multiCity)
        {
            List<OutAmadeusFlightSearchDTO?> newResponseList = new ();
            if (multiCity)
            {
                for(int i = 0; i < responsesList.Count/2; i++)
                {
                    newResponseList.Add(FlightMapper.MergeTwoMulticityItineraries(responsesList[2*i], responsesList[2*i + 1]));
                }
            }
            return multiCity ? newResponseList : responsesList;
        }

        public static FlightSegment OutFlightSegmentToDbSegment(this OutFlightSegmentDTO outFlightSegmentDTO)
        {
            return new FlightSegment{
                From = outFlightSegmentDTO.From,
                To = outFlightSegmentDTO.To,
                Departure = outFlightSegmentDTO.Departure,
                Arrival = outFlightSegmentDTO.Arrival,
                Duration = outFlightSegmentDTO.Duration,
                FlightCode = outFlightSegmentDTO.FlightCode
            };
        }

        public static Itinerary InItineraryToDbItinerary(this InItineraryDTO inItineraryDTO, string userId)
        {
            var isReturnFlight = !inItineraryDTO.OutFlightDealDTO.FromSegments.IsNullOrEmpty();
            var toSegmentLength = inItineraryDTO.OutFlightDealDTO.ToSegments.Count();
            var segments = isReturnFlight ? inItineraryDTO.OutFlightDealDTO.ToSegments.Concat(inItineraryDTO.OutFlightDealDTO.FromSegments??new List<OutFlightSegmentDTO>()) : inItineraryDTO.OutFlightDealDTO.ToSegments;
            Itinerary itinerary = new Itinerary {
                Adults = inItineraryDTO.Adults,
                TotalStayDuration = inItineraryDTO.OutFlightDealDTO.TotalStayDuration,
                ToDuration = inItineraryDTO.OutFlightDealDTO.ToDuration,
                Segments = segments.Select(segment => segment.OutFlightSegmentToDbSegment()).ToList(),
                ToSegmentsLength = toSegmentLength,
                LayoverToDuration = ListHelper.ListToString(inItineraryDTO.OutFlightDealDTO.LayoverToDuration),
                CityVisit = ListHelper.ListToString(inItineraryDTO.OutFlightDealDTO.CityVisit),
                FromDuration = inItineraryDTO.OutFlightDealDTO.FromDuration,
                LayoverFromDuration = ListHelper.ListToString(inItineraryDTO.OutFlightDealDTO.LayoverFromDuration),
                TotalPrice = inItineraryDTO.OutFlightDealDTO.TotalPrice,
                ChatGPTGeneratedText = inItineraryDTO.ChatGPTGeneratedText,
                PriceChangeNotificationType = inItineraryDTO.PriceChangeNotificationType,
                Percentage = inItineraryDTO.Percentage,
                Amount = inItineraryDTO.Amount,
                UserId = userId
            };
            return itinerary;
        }

    }
}