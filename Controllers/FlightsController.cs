using FlightSearch.DTOs.InModels;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FlightSearch.Mappers;
using FlightSearch.Enums;
using FlightSearch.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace FlightSearch.Controllers
{
    [Route("api/flights")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IFlightRepository _flightRepository;

        public FlightsController(ICountryRepository countryRepository, IFlightRepository flightRepository)
        {
            _countryRepository = countryRepository;
            _flightRepository = flightRepository;
        }

        [HttpGet("getTravelPairs")]
        [Authorize]
        public async Task<IActionResult> GetTravelDatePairs([FromQuery] InDatePairsDTO inDatePairsDTO)
        {
            if (inDatePairsDTO.TotalDays < inDatePairsDTO.DaysOff)
            {
                return BadRequest();
            }
            var countryId = await TokenHelper.GetCountryIdFromHttpContext(HttpContext);
            var country = await _countryRepository.GetCountryById(countryId);
            if (country == null)
            {
                return NotFound();
            }
            var publicHolidays = country.ToOutCountryDTO().PublicHolidays;
            var listOfDatePairs = PublicHolidayMapper.HolidaysToDatePairs(publicHolidays, inDatePairsDTO);
            return Ok(listOfDatePairs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchFlights([FromQuery] InFlightSearchDTO inFlightSearchDTO)
        {
            if (!inFlightSearchDTO.MulticityRightfullyDefined())
            {
                return BadRequest("Multicity not defined rightfully");
            }
            switch(inFlightSearchDTO.FlightSearchType)
            {
                case (FlightSearchType.ExactDate):
                {
                    if (!DateHelper.IsDateWithinYear(inFlightSearchDTO.DepartureDay))
                    {
                        return BadRequest("Departure date not within a year");
                    }
                    if (inFlightSearchDTO.ReturnDay != null && !DateHelper.IsDateWithinYear(inFlightSearchDTO.ReturnDay))
                    {
                        return BadRequest("Return date not within a year");
                    }
                    if (inFlightSearchDTO.ReturnDay != null && inFlightSearchDTO.DepartureDay >= inFlightSearchDTO.ReturnDay)
                    {
                        return BadRequest("Departure date before return date");
                    }
                    if (inFlightSearchDTO.MulticityDefined() && !DateHelper.IsDateWithinYear(inFlightSearchDTO.ReturnDay))
                    {
                        return BadRequest("Return date for multicity search required");
                    }
                    break;
                }
                case (FlightSearchType.MonthDirectFlight):
                case (FlightSearchType.DuratinInMonth):
                case (FlightSearchType.LongWeekendInMonth):
                case (FlightSearchType.DoubleLongWeekendInMonth):
                {
                    if (inFlightSearchDTO.FlightSearchType != FlightSearchType.MonthDirectFlight && inFlightSearchDTO.Month == null || inFlightSearchDTO.Month < 1 || inFlightSearchDTO.Month > 12 || inFlightSearchDTO.Year == null)
                    {
                        return BadRequest("Month and date are not valid");
                    }
                    if (inFlightSearchDTO.FlightSearchType == FlightSearchType.DuratinInMonth && (inFlightSearchDTO.TripDuration == null || inFlightSearchDTO.TripDuration < 1 || inFlightSearchDTO.TripDuration > 21))
                    {
                        return BadRequest("You need to provide trip duration(within 3 weeks)");
                    }
                    var today = DateHelper.Today();
                    var firstDayOfMonth = new DateOnly(inFlightSearchDTO.Year.GetValueOrDefault(), inFlightSearchDTO.Month.GetValueOrDefault(), inFlightSearchDTO.Year == today.Year && inFlightSearchDTO.Month == today.Month ? today.Day : 1); 
                    if (!DateHelper.IsDateWithinYear(firstDayOfMonth))
                    {
                        return BadRequest("First day of the month not within a year");
                    }
                    break;
                }
                default:
                {
                    return BadRequest("Bad FlightSearchType");
                }
            }
            var requests = inFlightSearchDTO.ToAmadeusFLightSearchDTOs();
            var returnData = await _flightRepository.GetFlightData(requests, inFlightSearchDTO.MulticityDefined());
            return Ok(returnData.ToFinalFlightData());
        }

    }
}