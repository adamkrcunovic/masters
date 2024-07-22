using FlightSearch.DTOs.InModels;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FlightSearch.Mappers;
using FlightSearch.DTOs.OutModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FlightSearch.Controllers
{
    [Route("api/Flights")]
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

        [HttpGet("getTravelPairs/{id}")]
        public async Task<IActionResult> GetTravelDatePairs([FromRoute] int id, [FromQuery] InDatePairsDTO inDatePairsDTO)
        {
            if (inDatePairsDTO.TotalDays < inDatePairsDTO.DaysOff)
            {
                return BadRequest();
            }
            var country = await _countryRepository.GetCountryById(id);
            if (country == null)
            {
                return NotFound();
            }
            var dateToday = DateOnly.FromDateTime(DateTime.Now);
            var publicHolidays = country.ToOutCountryDTO().PublicHolidays;
            var listOfDatePairs = new List<OutDatePairsDTO>();
            foreach (DateOnly publicHolidayDate in publicHolidays)
            {
                for(int i = 0; i < inDatePairsDTO.TotalDays; i++)
                {
                    DateOnly startingDate = publicHolidayDate.AddDays(-i);
                    DateOnly endDate = startingDate.AddDays(inDatePairsDTO.TotalDays - 1);
                    if (startingDate.CompareTo(dateToday) <= 0)
                    {
                        break;
                    }
                    int publicHolidayAndWeekendCount = 0;
                    DateOnly currentDate = startingDate;
                    List<DateOnly> publicHolidaysInRange = new List<DateOnly>();
                    List<DateOnly> weekendsInRange = new List<DateOnly>();
                    List<DateOnly> daysOffInRange = new List<DateOnly>();
                    while (currentDate.CompareTo(endDate) <= 0)
                    {
                        if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday || publicHolidays.Contains(currentDate))
                        {
                            publicHolidayAndWeekendCount++;
                            if (publicHolidays.Contains(currentDate))
                            {
                                publicHolidaysInRange.Add(currentDate);
                            }
                            else
                            {
                                weekendsInRange.Add(currentDate);
                            }
                        }
                        else
                        {
                            daysOffInRange.Add(currentDate);
                        }
                        currentDate = currentDate.AddDays(1);
                    }
                    if (inDatePairsDTO.TotalDays <= inDatePairsDTO.DaysOff + publicHolidayAndWeekendCount)
                    {
                        if (listOfDatePairs.Where(datePair => datePair.StartDate.Equals(startingDate)).ToList().Count == 0)
                        {
                            listOfDatePairs.Add(new OutDatePairsDTO()
                            {
                                StartDate = startingDate,
                                EndDate = endDate,
                                PublicHolidays = publicHolidaysInRange,
                                Weekends = weekendsInRange,
                                DaysOff = daysOffInRange
                            });
                        }
                    }
                }
            }
            listOfDatePairs.Sort((x, y) => x.StartDate.CompareTo(y.StartDate));
            return Ok(listOfDatePairs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchFlights([FromQuery] InFlightSearchDTO inFlightSearchDTO)
        {
            var requests = inFlightSearchDTO.ToAmadeusFLightSearchDTOs();
            var returnData = await _flightRepository.GetFlightData(requests);
            return Ok(returnData);
        }

    }
}