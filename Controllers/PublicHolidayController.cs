using FlightSearch.DTOs.InModels;
using FlightSearch.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlightSearch.Controllers
{
    [ApiController]
    [Route("api/publicHoliday")]
    public class PublicHolidayController : ControllerBase
    {
        private readonly ICountryRepository _countryReposiroty;
        private readonly IPublicHolidayRepository _publicHolidayReposiroty;

        public PublicHolidayController(ICountryRepository countryRepository, IPublicHolidayRepository publicHolidayRepository)
        {
            _countryReposiroty = countryRepository;
            _publicHolidayReposiroty = publicHolidayRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddPublicHoliday(InPublicHolidayDTO inPublicHolidayDTO)
        {
            var country = await _countryReposiroty.GetCountryById(inPublicHolidayDTO.CountryId);
            if (country == null)
            {
                return NotFound();
            }
            var publicHoliday = await _publicHolidayReposiroty.CreatePublicHoliday(inPublicHolidayDTO);
            return Ok();
        }
    }
}